using NovaCore.BuildingBlock.SharedKernel.Text;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

using NovaCore.User.Application.Abstractions.Search;

namespace NovaCore.User.Persistence.Contexts.Users.Search.Repositories;

/// <summary>
/// IUserSearchRepository impl - query-only against Elasticsearch, never Postgres. Composes
/// keyword + role + status + phone prefix/suffix + pagination + sort into one query, per the
/// task's "one Elasticsearch query instead of mixing Elasticsearch + PostgreSQL" requirement.
/// See docs/reference/search.md.
/// </summary>
public sealed class UserSearchRepository(ElasticsearchClient client) : IUserSearchRepository
{
    public async Task<(IReadOnlyList<UserSearchDocument> Items, long TotalCount)> SearchAsync(
        UserSearchCriteria criteria,
        CancellationToken ct = default)
    {
        var pageSize = Math.Max(criteria.PageSize, 1);
        var from = (Math.Max(criteria.Page, 1) - 1) * pageSize;

        var response = await client.SearchAsync<UserSearchDocument>(s => s
            .Indices(UserSearchIndexNames.Default)
            .Query(q => q.Bool(b => BuildBoolQuery(b, criteria)))
            .Sort(BuildSort(criteria))
            .From(from)
            .Size(pageSize), ct);

        if (!response.IsValidResponse)
            throw new InvalidOperationException($"Elasticsearch search failed: {response.DebugInformation}");

        var documents = response.Documents?.ToList() ?? [];

        long totalCount = documents.Count;
        if (response.HitsMetadata?.Total is { } total)
        {
            // Union<TotalHits, long>.Match is nullable-oblivious (external, unannotated assembly) -
            // the pattern match above already guarantees `total` is non-null.
#pragma warning disable CS8602
            totalCount = total.Match(hits => hits.Value, count => count);
#pragma warning restore CS8602
        }

        return (documents, totalCount);
    }

    private static void BuildBoolQuery(BoolQueryDescriptor<UserSearchDocument> query, UserSearchCriteria criteria)
    {
        var must = new List<Action<QueryDescriptor<UserSearchDocument>>>();
        var filter = new List<Action<QueryDescriptor<UserSearchDocument>>>();
        var mustNot = new List<Action<QueryDescriptor<UserSearchDocument>>>();

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var keyword = criteria.Keyword;
            must.Add(q => q.MultiMatch(m => m
                .Fields(new[] { "searchName", "userName", "email" })
                .Query(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Role))
        {
            var role = criteria.Role;
            var roleClause = (Action<QueryDescriptor<UserSearchDocument>>)(q => q.Term(t => t.Field("roles").Value(role)));
            if (criteria.ExcludeRole)
                mustNot.Add(roleClause);
            else
                filter.Add(roleClause);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Status))
        {
            var status = criteria.Status;
            filter.Add(q => q.Term(t => t.Field("status").Value(status)));
        }

        // Same "prefix on PhoneSearch / prefix on the reversed PhoneReverse" trick the Postgres
        // path uses (NovaCore.BuildingBlock.Criteria's PhoneSearchStrategy) - both index-backed, never a
        // full scan. PhoneNormalizer is reused here for parity: the same normalization the
        // domain entity uses to populate PhoneSearch/PhoneReverse in the first place.
        if (!string.IsNullOrWhiteSpace(criteria.PhonePrefix))
        {
            var prefix = PhoneNormalizer.Normalize(criteria.PhonePrefix);
            filter.Add(q => q.Prefix(p => p.Field("phoneSearch").Value(prefix)));
        }

        if (!string.IsNullOrWhiteSpace(criteria.PhoneSuffix))
        {
            var reversedSuffix = PhoneNormalizer.Reverse(PhoneNormalizer.Normalize(criteria.PhoneSuffix));
            filter.Add(q => q.Prefix(p => p.Field("phoneReverse").Value(reversedSuffix)));
        }

        if (must.Count > 0)
            query.Must(must.ToArray());

        if (filter.Count > 0)
            query.Filter(filter.ToArray());

        if (mustNot.Count > 0)
            query.MustNot(mustNot.ToArray());
    }

    // Extending sortable fields later is just a new case here - no redesign.
    private static Action<SortOptionsDescriptor<UserSearchDocument>>[] BuildSort(UserSearchCriteria criteria)
    {
        var direction = criteria.SortDescending ? SortOrder.Desc : SortOrder.Asc;

        var field = criteria.SortBy?.Trim().ToLowerInvariant() switch
        {
            "username" => "userName.keyword",
            "email" => "email.keyword",
            "createdat" => "createdAt",
            "updatedat" => "updatedAt",
            _ => null,
        };

        return field is null ? [] : [s => s.Field(field, direction)];
    }
}
