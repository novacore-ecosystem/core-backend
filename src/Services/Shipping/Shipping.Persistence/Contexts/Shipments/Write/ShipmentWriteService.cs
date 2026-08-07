using NovaCore.Shipping.Application.Abstractions.Persistence.Shipments;
using NovaCore.Shipping.Persistence.Contexts.Shipments.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.Shipments.Write;

public sealed class ShipmentWriteService(IShipmentRepository repo) : IShipmentWriteService
{
    public async Task<Shipment> CreateAsync(
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
        CancellationToken ct = default)
    {
        var shipment = Shipment.Create(
            shipmentType,
            sourceType,
            sourceReferenceId,
            senderName,
            senderPhone,
            senderAddress,
            receiverName,
            receiverPhone,
            receiverAddress,
            declaredValue,
            requestedPickupAt,
            expectedDeliveryAt,
            note,
            idempotencyKey);

        await repo.AddAsync(shipment, ct);

        return shipment;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
