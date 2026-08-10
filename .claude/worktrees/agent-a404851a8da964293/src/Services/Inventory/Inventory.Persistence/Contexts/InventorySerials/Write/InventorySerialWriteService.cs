using NovaCore.Inventory.Application.Abstractions.Persistence.InventorySerials;
using NovaCore.Inventory.Application.Features.InventorySerials.DTOs;
using NovaCore.Inventory.Persistence.Contexts.InventorySerials.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventorySerials.Write;

/// <summary>
/// Never calls IUnitOfWork itself - always invoked from within a caller-owned
/// ExecuteTransactionAsync, which performs the single SaveChanges.
/// </summary>
public sealed class InventorySerialWriteService(
    IInventorySerialRepository repo) : IInventorySerialWriteService
{
    public async Task AddAsync(CreateInventorySerialRequest request, CancellationToken ct = default)
    {
        var entity = InventorySerial.Create(request.InventoryId, request.SerialNumber);

        await repo.AddAsync(entity, ct);
    }

    public async Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        await repo.DeleteWithNoTrackingAsync(s => s.InventoryId == inventoryId, ct);
    }
}
