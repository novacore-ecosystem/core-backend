using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryLots;
using NovaCore.Inventory.Persistence.Contexts.InventoryLots.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryLots.Read;

public sealed class InventoryLotReadService(IInventoryLotRepository repo) : IInventoryLotReadService
{
    public async Task<InventoryLot?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(id, ct);
    }

    public async Task<PaginatedResult<InventoryLot>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await repo.SearchAsync(request, ct);
    }

    public async Task<IReadOnlyList<InventoryLot>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        return await repo.GetByInventoryIdAsync(inventoryId, ct);
    }
}
