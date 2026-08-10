using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Product.Persistence.Contexts.Products.Repositories;

public interface IProductRepository : IRepository<ProductEntity>
{
    Task<ProductEntity[]> GetAllAsync(
        int skip,
        int take,
        CancellationToken ct = default);

    Task<Guid[]> GetProductsByTagIdAsync(
        Guid tagId,
        CancellationToken ct = default);

    Task<int> GetNextVariationDisplayOrderAsync(
        Guid productId,
        CancellationToken ct = default);

    Task AddVariationAsync(
        ProductVariant variation,
        CancellationToken ct = default);

    Task AddVariationRangeAsync(
        IEnumerable<ProductVariant> variations,
        CancellationToken ct = default);

    Task UpdateVariationAsync(
        Guid id,
        Func<IQueryable<ProductVariant>, IQueryable<ProductVariant>> includes,
        Func<ProductVariant, Task> updateAction,
        CancellationToken ct = default);

    Task RemoveVariationAsync(Guid id, CancellationToken ct = default);
}
