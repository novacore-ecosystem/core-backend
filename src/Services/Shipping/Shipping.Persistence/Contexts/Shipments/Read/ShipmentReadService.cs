using NovaCore.Shipping.Application.Abstractions.Persistence.Shipments;
using NovaCore.Shipping.Persistence.Contexts.Shipments.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.Shipments.Read;

public sealed class ShipmentReadService(IShipmentRepository repo) : IShipmentReadService
{
    /// <summary>Items/Events/Packages are relational children, never auto-loaded - every read path Includes them explicitly.</summary>
    public async Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(
            id,
            query => query
                .Include(s => s.Items)
                .Include(s => s.Events)
                .Include(s => s.Packages).ThenInclude(p => p.Items),
            ct);

    public async Task<Shipment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
        => await repo.GetByIdempotencyKeyAsync(idempotencyKey, ct);

    public async Task<Shipment?> GetByShipmentNumberAsync(string shipmentNumber, CancellationToken ct = default)
        => await repo.GetByShipmentNumberAsync(shipmentNumber, ct);
}
