using NovaCore.BuildingBlock.Search.Abstractions;

using NovaCore.Product.Application.Abstractions.Search;
using NovaCore.Product.Persistence.Contexts.Products.Search.Mapping;

namespace NovaCore.Product.Persistence.Contexts.Products.Search.Indexers;

/// <summary>
/// IProductSearchIndexer impl - fixes the Product index name/mapping on top of
/// NovaCore.BuildingBlock.Search's generic, reusable IElasticsearchIndexer&lt;&gt;. The name passed
/// through here is an ES alias, not a concrete index - EnsureIndexAsync/RecreateIndexAsync
/// manage the versioned index + alias swap underneath; this class stays unaware of that detail.
/// </summary>
public sealed class ProductSearchIndexer(IElasticsearchIndexer<ProductSearchDocument> indexer) : IProductSearchIndexer
{
    public Task EnsureIndexAsync(CancellationToken ct = default) =>
        indexer.EnsureIndexAsync(ProductSearchIndexNames.Default, ProductSearchIndexMapping.Configure, ct);

    public Task RecreateIndexAsync(CancellationToken ct = default) =>
        indexer.RecreateIndexAsync(ProductSearchIndexNames.Default, ProductSearchIndexMapping.Configure, ct);

    public Task IndexAsync(ProductSearchDocument document, CancellationToken ct = default) =>
        indexer.IndexAsync(ProductSearchIndexNames.Default, document.ProductId.ToString(), document, ct);

    public Task DeleteAsync(Guid productId, CancellationToken ct = default) =>
        indexer.DeleteAsync(ProductSearchIndexNames.Default, productId.ToString(), ct);

    public Task BulkIndexAsync(IEnumerable<ProductSearchDocument> documents, CancellationToken ct = default) =>
        indexer.BulkIndexAsync(
            ProductSearchIndexNames.Default,
            documents.Select(d => (d.ProductId.ToString(), d)),
            ct);
}
