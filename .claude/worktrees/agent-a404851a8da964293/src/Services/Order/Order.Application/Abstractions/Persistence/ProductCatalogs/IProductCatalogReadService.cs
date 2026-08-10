namespace NovaCore.Order.Application.Abstractions.Persistence.ProductCatalogs;

public interface IProductCatalogReadService
{
    Task<ProductCatalog[]> GetByVariantionIdsAsync(Guid[] variationIds, CancellationToken ct = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
