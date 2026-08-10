using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Inventory.Application.Abstractions.Persistence.InventoryCounts;

public interface IInventoryCountReadService
{
    Task<InventoryCount?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<InventoryCount>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);

    Task<InventoryCount?> GetByNumberAsync(
        string number,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryCount>> GetByWarehouseIdAsync(
        Guid warehouseId,
        CancellationToken ct = default);
}
