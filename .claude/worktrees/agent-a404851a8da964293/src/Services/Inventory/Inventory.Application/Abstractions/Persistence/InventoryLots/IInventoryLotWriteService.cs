namespace NovaCore.Inventory.Application.Abstractions.Persistence.InventoryLots;

public interface IInventoryLotWriteService
{
    Task AddAsync(CreateInventoryLotRequest request, CancellationToken ct = default);

    Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default);
}
