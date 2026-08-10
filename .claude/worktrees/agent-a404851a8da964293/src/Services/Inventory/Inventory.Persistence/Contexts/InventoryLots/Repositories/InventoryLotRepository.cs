using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Ef.Criteria;
using NovaCore.Inventory.Application.Features.Inventories.Search;
using NovaCore.Inventory.Application.Features.InventoryLots.Search;
using NovaCore.Inventory.Persistence.Engine;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryLots.Repositories;

public sealed class InventoryLotRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventoryLot, Guid>(dbContext), IInventoryLotRepository
{
    public async Task<IReadOnlyList<InventoryLot>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryLots
            .AsNoTracking()
            .Where(l => l.InventoryId == inventoryId)
            .ToListAsync(ct);
    }

    public async Task<PaginatedResult<InventoryLot>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryLots
            .AsNoTracking()
            .ApplyCriteria(InventoryLotCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
