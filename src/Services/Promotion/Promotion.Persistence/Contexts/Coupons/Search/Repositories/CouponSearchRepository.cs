using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

using NovaCore.Promotion.Application.Abstractions.Search;

namespace NovaCore.Promotion.Persistence.Contexts.Coupons.Search.Repositories;

/// <summary>
/// ICouponSearchRepository impl - query-only against Elasticsearch, never Postgres. Tenant
/// isolation (Section 11) is always applied as a mandatory filter, unlike every other criterion
/// here. Fuzziness on the keyword MultiMatch is this codebase's first real fuzzy-search usage -
/// ProductSearch/UserSearch have no fuzziness precedent to clone (verified by reading both), but
/// Phase 3.4's own brief names fuzzy discovery as this integration's primary purpose, so this adds
/// the standard Elasticsearch "AUTO" fuzziness to the existing Bool/MultiMatch query shape rather
/// than leaving it unimplemented. See docs/promotion-service/search/search-strategy.md.
/// </summary>
public sealed class CouponSearchRepository(ElasticsearchClient client) : ICouponSearchRepository
{
    public async Task<(IReadOnlyList<CouponSearchDocument> Items, long TotalCount)> SearchAsync(
        CouponSearchCriteria criteria,
        CancellationToken ct = default)
    {
        var pageSize = Math.Max(criteria.PageSize, 1);
        var from = (Math.Max(criteria.Page, 1) - 1) * pageSize;

        var response = await client.SearchAsync<CouponSearchDocument>(s => s
            .Indices(CouponSearchIndexNames.Default)
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

    private static void BuildBoolQuery(BoolQueryDescriptor<CouponSearchDocument> query, CouponSearchCriteria criteria)
    {
        var must = new List<Action<QueryDescriptor<CouponSearchDocument>>>();
        var filter = new List<Action<QueryDescriptor<CouponSearchDocument>>>();

        var tenantId = criteria.TenantId.ToString();
        filter.Add(q => q.Term(t => t.Field("tenantId").Value(tenantId)));

        // Public visibility (Phase 4.3, Section 12): a disabled Coupon must never surface in
        // public search regardless of caller-supplied criteria - unconditional, same trust
        // boundary as TenantId above, not exposed as a criteria field.
        filter.Add(q => q.Term(t => t.Field("isEnabled").Value(true)));

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var keyword = criteria.Keyword;
            must.Add(q => q.MultiMatch(m => m
                .Fields(new[] { "code", "name", "translatedNames" })
                .Query(keyword)
                .Fuzziness(new Fuzziness("AUTO"))));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Status))
        {
            var status = criteria.Status;
            filter.Add(q => q.Term(t => t.Field("status").Value(status)));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Visibility))
        {
            var visibility = criteria.Visibility;
            filter.Add(q => q.Term(t => t.Field("visibility").Value(visibility)));
        }

        if (criteria.AvailableAsOf is { } asOf)
        {
            filter.Add(q => q.Range(r => r.DateRange(dr => dr.Field("startTime").Lte(asOf))));
            filter.Add(q => q.Range(r => r.DateRange(dr => dr.Field("endTime").Gte(asOf))));
        }

        if (must.Count > 0)
            query.Must(must.ToArray());

        if (filter.Count > 0)
            query.Filter(filter.ToArray());
    }

    // Extending sortable fields later is just a new case here - no redesign.
    private static Action<SortOptionsDescriptor<CouponSearchDocument>>[] BuildSort(CouponSearchCriteria criteria)
    {
        var direction = criteria.SortDescending ? SortOrder.Desc : SortOrder.Asc;

        var field = criteria.SortBy?.Trim().ToLowerInvariant() switch
        {
            "name" => "name.keyword",
            "code" => "code.keyword",
            "starttime" => "startTime",
            "endtime" => "endTime",
            "updatedat" => "updatedAt",
            _ => null,
        };

        return field is null ? [] : [s => s.Field(field, direction)];
    }
}
