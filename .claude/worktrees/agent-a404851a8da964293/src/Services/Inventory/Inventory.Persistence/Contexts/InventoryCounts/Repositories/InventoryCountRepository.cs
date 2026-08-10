using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Ef.Criteria;
using NovaCore.Inventory.Application.Features.InventoryCounts.Search;
using NovaCore.Inventory.Application.Features.Inventories.Search;
using NovaCore.Inventory.Persistence.Engine;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryCounts.Repositories;

public sealed class InventoryCountRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventoryCount, Guid>(dbContext), IInventoryCountRepository
{
    public async Task<InventoryCount?> GetByNumberAsync(
        string number,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryCounts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Number == number, ct);
    }

    public async Task<IReadOnlyList<InventoryCount>> GetByWarehouseIdAsync(
        Guid warehouseId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryCounts
            .AsNoTracking()
            .Where(c => c.WarehouseId == warehouseId)
            .ToListAsync(ct);
    }

    public async Task<PaginatedResult<InventoryCount>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryCounts
            .AsNoTracking()
            .ApplyCriteria(InventoryCountCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
