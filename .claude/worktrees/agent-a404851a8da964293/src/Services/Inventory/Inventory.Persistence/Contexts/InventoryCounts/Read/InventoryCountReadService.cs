using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryCounts;
using NovaCore.Inventory.Persistence.Contexts.InventoryCounts.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryCounts.Read;

public sealed class InventoryCountReadService(IInventoryCountRepository repo) : IInventoryCountReadService
{
    public async Task<InventoryCount?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(id, ct);
    }

    public async Task<PaginatedResult<InventoryCount>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await repo.SearchAsync(request, ct);
    }

    public async Task<InventoryCount?> GetByNumberAsync(string number, CancellationToken ct = default)
    {
        return await repo.GetByNumberAsync(number, ct);
    }

    public async Task<IReadOnlyList<InventoryCount>> GetByWarehouseIdAsync(Guid warehouseId, CancellationToken ct = default)
    {
        return await repo.GetByWarehouseIdAsync(warehouseId, ct);
    }
}
