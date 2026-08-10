using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

using NovaCore.Product.Application.Abstractions.Search;

namespace NovaCore.Product.Persistence.Contexts.Products.Search.Repositories;

/// <summary>
/// IProductSearchRepository impl - query-only against Elasticsearch, never Postgres. Builds the
/// query DSL directly against ElasticsearchClient (too Product-specific to push into
/// NovaCore.BuildingBlock.Search). See docs/reference/search.md.
/// </summary>
public sealed class ProductSearchRepository(ElasticsearchClient client) : IProductSearchRepository
{
    public async Task<(IReadOnlyList<ProductSearchDocument> Items, long TotalCount)> SearchAsync(
        ProductSearchCriteria criteria,
        CancellationToken ct = default)
    {
        var pageSize = Math.Max(criteria.PageSize, 1);
        var from = (Math.Max(criteria.Page, 1) - 1) * pageSize;

        var response = await client.SearchAsync<ProductSearchDocument>(s => s
            .Indices(ProductSearchIndexNames.Default)
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

    private static void BuildBoolQuery(BoolQueryDescriptor<ProductSearchDocument> query, ProductSearchCriteria criteria)
    {
        var must = new List<Action<QueryDescriptor<ProductSearchDocument>>>();
        var filter = new List<Action<QueryDescriptor<ProductSearchDocument>>>();

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var keyword = criteria.Keyword;
            must.Add(q => q.MultiMatch(m => m
                .Fields(new[] { "name", "variationNames", "categoryNames", "tagNames" })
                .Query(keyword)));
        }

        if (criteria.CategoryId is not null)
        {
            var categoryId = criteria.CategoryId.Value.ToString();
            filter.Add(q => q.Term(t => t.Field("categoryIds").Value(categoryId)));
        }

        if (criteria.TagId is not null)
        {
            var tagId = criteria.TagId.Value.ToString();
            filter.Add(q => q.Term(t => t.Field("tagIds").Value(tagId)));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Status))
        {
            var status = criteria.Status;
            filter.Add(q => q.Term(t => t.Field("status").Value(status)));
        }

        if (must.Count > 0)
            query.Must(must.ToArray());

        if (filter.Count > 0)
            query.Filter(filter.ToArray());
    }

    // Extending sortable fields later (e.g. Brand) is just a new case here - no redesign.
    private static Action<SortOptionsDescriptor<ProductSearchDocument>>[] BuildSort(ProductSearchCriteria criteria)
    {
        var direction = criteria.SortDescending ? SortOrder.Desc : SortOrder.Asc;

        var field = criteria.SortBy?.Trim().ToLowerInvariant() switch
        {
            "name" => "name.keyword",
            "price" => "defaultPrice",
            "updatedat" => "updatedAt",
            _ => null,
        };

        return field is null ? [] : [s => s.Field(field, direction)];
    }
}
