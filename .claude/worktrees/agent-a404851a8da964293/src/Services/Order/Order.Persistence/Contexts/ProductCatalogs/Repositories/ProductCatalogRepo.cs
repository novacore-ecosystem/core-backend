using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Contexts.ProductCatalogs.Repositories;

public sealed class ProductCatalogRepo(OrderDbContext dbContext)
    : OrderBaseRepository<ProductCatalog>(dbContext), IProductCatalogRepository
{
    public async Task UpdateAsync(
        Guid productId,
        Guid variationId,
        Action<ProductCatalog> updateAction,
        CancellationToken ct = default)
    {
        var entry = await _dbContext.ProductCatalogs
            .FirstOrDefaultAsync(p => p.ProductId == productId && p.VariationId == variationId, ct)
            ?? throw new NotFoundException(nameof(ProductCatalog), variationId);

        updateAction?.Invoke(entry);
    }

    public async Task DeleteAsync(
        Guid productId,
        Guid variationId,
        CancellationToken ct = default)
    {
        await _dbContext.ProductCatalogs
            .Where(p => p.ProductId == productId && p.VariationId == variationId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken ct = default)
    {
        await _dbContext.ProductCatalogs
            .Where(p => p.ProductId == productId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<int> UpdateProductNameByProductIdAsync(
        Guid productId,
        string name,
        CancellationToken ct = default)
    {
        return await _dbContext.ProductCatalogs
            .Where(p => p.ProductId == productId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.ProductName, name), ct);
    }
}
