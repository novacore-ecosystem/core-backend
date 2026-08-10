namespace NovaCore.Shipping.Application.Abstractions.Persistence.Pickups;

public interface IPickupReadService
{
    Task<Pickup?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Pickup>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken ct = default);
}
