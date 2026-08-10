namespace NovaCore.Inventory.Application.Abstractions.Persistence.InventoryReservations;

public interface IInventoryReservationWriteService
{
    Task AddAsync(CreateInventoryReservationRequest request, CancellationToken ct = default);

    Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default);
}
