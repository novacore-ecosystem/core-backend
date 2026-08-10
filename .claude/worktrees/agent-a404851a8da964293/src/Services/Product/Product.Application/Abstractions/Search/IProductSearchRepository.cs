namespace NovaCore.Product.Application.Abstractions.Search;

/// <summary>
/// Query-only access to the Product Search index. Deliberately exposes no Add/Update/Delete -
/// indexing is IProductSearchIndexer's responsibility, never this repository's. See
/// docs/reference/search.md.
/// </summary>
public interface IProductSearchRepository
{
    Task<(IReadOnlyList<ProductSearchDocument> Items, long TotalCount)> SearchAsync(
        ProductSearchCriteria criteria, CancellationToken ct = default);
}
