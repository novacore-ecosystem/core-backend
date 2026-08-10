namespace NovaCore.Shipping.Application.Abstractions.Persistence.Pickups;

public interface IPickupWriteService
{

    Task<Pickup> CreateAsync(
        Guid shipmentId,
        PickupType pickupType,
        ShippingAddress address,
        string contactName,
        PhoneNumber contactPhone,
        DateTime scheduledAt,
        string? note = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
