using NovaCore.Order.Application.Abstractions.Persistence.ProductCatalogs;
using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Contexts.ProductCatalogs.Read;

public sealed class ProductCatalogReadService(OrderDbContext dbContext) : IProductCatalogReadService
{
    public async Task<ProductCatalog[]> GetByVariantionIdsAsync(Guid[] variationIds, CancellationToken ct = default)
    {
        return await dbContext.ProductCatalogs
            .AsNoTracking()
            .Where(opc => variationIds.Contains(opc.Id))
            .ToArrayAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.ProductCatalogs
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, ct);
    }
}
