using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Inventory.Application.Abstractions.Persistence.InventoryReservations;

public interface IInventoryReservationReadService
{
    Task<InventoryReservation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<InventoryReservation>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);

    Task<InventoryReservation?> GetByNumberAsync(
        string number,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryReservation>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);

    Task<IReadOnlyList<InventoryReservation>> GetActiveByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default);
}
