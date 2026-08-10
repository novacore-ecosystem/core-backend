namespace NovaCore.Order.Application.Abstractions.Persistence.ProductCatalogs;

public interface IProductCatalogWriteService
{
    Task CreateAsync(ProductCatalog entry, CancellationToken ct = default);

    /// <summary>Refreshes the full variation snapshot (name + pricing) - used when a variation-created event arrives for an id this catalog already has a row for.</summary>
    Task UpdateVariationSnapshotAsync(
        Guid productId,
        Guid variationId,
        string name,
        Sku sku,
        Money price,
        ProductCatalogStatus status,
        CancellationToken ct = default);

    Task DeleteAsync(
        Guid productId,
        Guid variationId,
        CancellationToken ct = default);

    /// <summary>Bulk-refreshes ProductName across every catalog row for this product - a Product's name is shared by every variation.</summary>
    Task UpdateProductNameByProductIdAsync(
        Guid productId,
        string name,
        CancellationToken ct = default);

    Task DeleteByProductIdAsync(
        Guid productId,
        CancellationToken ct = default);
}
