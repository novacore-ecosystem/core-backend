namespace NovaCore.Inventory.Application.Abstractions.Persistence.InventorySerials;

public interface IInventorySerialWriteService
{
    Task AddAsync(CreateInventorySerialRequest request, CancellationToken ct = default);

    Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default);
}
