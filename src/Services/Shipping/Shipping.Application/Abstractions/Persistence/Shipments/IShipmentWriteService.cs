namespace NovaCore.Shipping.Application.Abstractions.Persistence.Shipments;

public interface IShipmentWriteService
{

    /// <summary>
    /// Persists a new Shipment in Draft. Returns the aggregate itself (not a narrower DTO) because
    /// the caller needs its Id/ShipmentNumber to build the outgoing integration event - same
    /// reasoning as Order's IOrderWriteService.CreateAsync.
    /// </summary>
    Task<Shipment> CreateAsync(
        ShipmentType shipmentType,
        SourceType sourceType,
        Guid sourceReferenceId,
        string senderName,
        PhoneNumber senderPhone,
        ShippingAddress senderAddress,
        string receiverName,
        PhoneNumber receiverPhone,
        ShippingAddress receiverAddress,
        Money declaredValue,
        DateTime? requestedPickupAt = null,
        DateTime? expectedDeliveryAt = null,
        string? note = null,
        string? idempotencyKey = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
