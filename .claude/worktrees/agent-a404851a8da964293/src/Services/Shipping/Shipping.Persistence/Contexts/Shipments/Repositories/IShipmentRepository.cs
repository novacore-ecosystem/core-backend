using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.Shipments.Repositories;

public interface IShipmentRepository : IRepository<Shipment, Guid>
{
    /// <summary>Backs the create-shipment idempotency check - see IShipmentReadService.GetByIdempotencyKeyAsync.</summary>
    Task<Shipment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);

    Task<Shipment?> GetByShipmentNumberAsync(string shipmentNumber, CancellationToken ct = default);
}
