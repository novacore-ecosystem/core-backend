using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryCounts;
using NovaCore.Inventory.Persistence.Contexts.InventoryCounts.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryCounts.Write;

/// <summary>
/// Never calls IUnitOfWork itself - always invoked from within a caller-owned
/// ExecuteTransactionAsync (StartCycleCountHandler/CompleteCycleCountHandler), which performs
/// the single SaveChanges.
/// </summary>
public sealed class InventoryCountWriteService(
    IInventoryCountRepository repo) : IInventoryCountWriteService
{
    public async Task AddAsync(InventoryCount entity, CancellationToken ct = default)
    {
        await repo.AddAsync(entity, ct);
    }

    public async Task UpdateAsync(Guid id, Action<InventoryCount> updateAction, CancellationToken ct = default)
    {
        await repo.UpdateAsync(id, updateAction, ct);
    }
}
