using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Inventory.Application.Abstractions.Persistence.InventoryLots;

public interface IInventoryLotReadService
{
    Task<InventoryLot?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<InventoryLot>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryLot>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);
}
