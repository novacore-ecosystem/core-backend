using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Inventory.Application.Abstractions.Persistence.InventorySerials;

public interface IInventorySerialReadService
{
    Task<InventorySerial?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<InventorySerial>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);

    Task<InventorySerial?> GetBySerialNumberAsync(
        string serialNumber,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventorySerial>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventorySerial>> GetAvailableByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);
}
