using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryReservations;
using NovaCore.Inventory.Application.Features.InventoryReservations.DTOs;
using NovaCore.Inventory.Persistence.Contexts.InventoryReservations.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryReservations.Write;

/// <summary>
/// Never calls IUnitOfWork itself - always invoked from within a caller-owned
/// ExecuteTransactionAsync, which performs the single SaveChanges.
/// </summary>
public sealed class InventoryReservationWriteService(
    IInventoryReservationRepository repo) : IInventoryReservationWriteService
{
    public async Task AddAsync(CreateInventoryReservationRequest request, CancellationToken ct = default)
    {
        var entity = InventoryReservation.Create(
            request.Number,
            request.Type,
            request.InventoryId,
            request.WarehouseId,
            request.ProductId,
            request.VariantId,
            request.Quantity,
            request.ReferenceType,
            request.ReferenceId,
            request.ExternalReference,
            request.ExpiredAt,
            request.Reason);

        await repo.AddAsync(entity, ct);
    }

    public async Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        await repo.DeleteWithNoTrackingAsync(r => r.InventoryId == inventoryId, ct);
    }
}
