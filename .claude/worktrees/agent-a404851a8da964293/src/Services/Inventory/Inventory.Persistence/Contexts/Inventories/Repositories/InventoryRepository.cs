using NovaCore.BuildingBlock.Persistence.Ef.Criteria;
using NovaCore.Inventory.Application.Features.Inventories.Search;
using NovaCore.Inventory.Persistence.Engine;

namespace NovaCore.Inventory.Persistence.Contexts.Inventories.Repositories;

public sealed class InventoryRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventoryStock, Guid>(dbContext), IInventoryRepository
{
    public async Task<PaginatedResult<InventoryStock>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryStocks
            .AsNoTracking()
            .ApplyCriteria(InventoryCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }

    public async Task DeleteByProductIdAsync(
        Guid productId,
        CancellationToken ct = default)
    {
        var rows = await _dbContext.InventoryStocks
            .Where(i => i.ProductId == productId)
            .ToListAsync(ct);

        _dbContext.InventoryStocks.RemoveRange(rows);
    }

    public async Task DeleteByVariationIdAsync(
        Guid productVariationId,
        CancellationToken ct = default)
    {
        var rows = await _dbContext.InventoryStocks
            .Where(i => i.VariantId == productVariationId)
            .ToListAsync(ct);

        _dbContext.InventoryStocks.RemoveRange(rows);
    }

    public async Task<int> GetTotalStockByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        return await _dbContext.InventoryStocks
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .SumAsync(i => i.OnHandQuantity.Value, ct);
    }

    public async Task<int> GetTotalStockByVariationIdAsync(Guid variationId, CancellationToken ct = default)
    {
        return await _dbContext.InventoryStocks
            .AsNoTracking()
            .Where(i => i.VariantId == variationId)
            .SumAsync(i => i.OnHandQuantity.Value, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetTotalStockByVariationIdsAsync(
        IReadOnlyCollection<Guid> variationIds,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryStocks
            .AsNoTracking()
            .Where(i => variationIds.Contains(i.VariantId))
            .GroupBy(i => i.VariantId)
            .Select(g => new { VariationId = g.Key, Total = g.Sum(i => i.OnHandQuantity.Value) })
            .ToDictionaryAsync(x => x.VariationId, x => x.Total, ct);
    }
}
