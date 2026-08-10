using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryLots;
using NovaCore.Inventory.Application.Features.InventoryLots.DTOs;
using NovaCore.Inventory.Persistence.Contexts.InventoryLots.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryLots.Write;

/// <summary>
/// Never calls IUnitOfWork itself - always invoked from within a caller-owned
/// ExecuteTransactionAsync (e.g. ReceivingService), which performs the single SaveChanges.
/// </summary>
public sealed class InventoryLotWriteService(
    IInventoryLotRepository repo) : IInventoryLotWriteService
{
    public async Task AddAsync(CreateInventoryLotRequest request, CancellationToken ct = default)
    {
        var entity = InventoryLot.Create(
            request.InventoryId,
            request.LotNumber,
            request.ManufactureDate,
            request.ExpiredDate,
            request.Quantity,
            request.SupplierLotNumber,
            request.CountryOfOrigin);

        await repo.AddAsync(entity, ct);
    }

    public async Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        await repo.DeleteWithNoTrackingAsync(l => l.InventoryId == inventoryId, ct);
    }
}
