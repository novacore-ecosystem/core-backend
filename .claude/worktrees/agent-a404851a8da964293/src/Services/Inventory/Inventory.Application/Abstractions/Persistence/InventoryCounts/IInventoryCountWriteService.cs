namespace NovaCore.Inventory.Application.Abstractions.Persistence.InventoryCounts;

public interface IInventoryCountWriteService
{
    Task AddAsync(InventoryCount entity, CancellationToken ct = default);

    Task UpdateAsync(Guid id, Action<InventoryCount> updateAction, CancellationToken ct = default);
}
