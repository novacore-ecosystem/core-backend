namespace NovaCore.Shipping.Application.Abstractions.Persistence.Shipments;

public interface IShipmentReadService
{
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Backs the create-shipment idempotency check - a retried request with the same key resolves to the original shipment instead of a duplicate.</summary>
    Task<Shipment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);

    Task<Shipment?> GetByShipmentNumberAsync(string shipmentNumber, CancellationToken ct = default);
}
