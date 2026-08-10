namespace NovaCore.Product.Application.Abstractions.Search;

/// <summary>
/// Write-only access to the Product Search index - the only place Product's search sync/rebuild
/// code is allowed to mutate Elasticsearch. Wraps NovaCore.BuildingBlock.Search's generic indexer with the
/// Product index name/mapping. See docs/reference/search.md.
/// </summary>
public interface IProductSearchIndexer
{
    /// <summary>Idempotent - creates the index+mapping only if missing. Called on every service startup.</summary>
    Task EnsureIndexAsync(CancellationToken ct = default);

    /// <summary>Drops and recreates the index - used only by the rebuild flow.</summary>
    Task RecreateIndexAsync(CancellationToken ct = default);

    Task IndexAsync(ProductSearchDocument document, CancellationToken ct = default);

    Task DeleteAsync(Guid productId, CancellationToken ct = default);

    Task BulkIndexAsync(IEnumerable<ProductSearchDocument> documents, CancellationToken ct = default);
}
