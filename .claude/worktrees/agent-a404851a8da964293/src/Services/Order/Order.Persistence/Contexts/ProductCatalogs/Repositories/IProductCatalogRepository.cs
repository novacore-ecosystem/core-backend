using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Order.Persistence.Contexts.ProductCatalogs.Repositories;

public interface IProductCatalogRepository : IRepository<ProductCatalog>
{
    Task UpdateAsync(
        Guid productId,
        Guid variationId,
        Action<ProductCatalog> updateAction,
        CancellationToken ct = default);

    Task DeleteAsync(
        Guid productId,
        Guid variationId,
        CancellationToken ct = default);

    Task DeleteProductAsync(
        Guid productId,
        CancellationToken ct = default);

    Task<int> UpdateProductNameByProductIdAsync(
        Guid productId,
        string name,
        CancellationToken ct = default);
}
