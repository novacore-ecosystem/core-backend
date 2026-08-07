using NovaCore.Shipping.Application.Abstractions.Persistence.Pickups;
using NovaCore.Shipping.Persistence.Contexts.Pickups.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.Pickups.Write;

public sealed class PickupWriteService(IPickupRepository repo) : IPickupWriteService
{
    public async Task<Pickup> CreateAsync(
        Guid shipmentId,
        PickupType pickupType,
        ShippingAddress address,
        string contactName,
        PhoneNumber contactPhone,
        DateTime scheduledAt,
        string? note = null,
        CancellationToken ct = default)
    {
        var pickup = Pickup.Create(shipmentId, pickupType, address, contactName, contactPhone, scheduledAt, note);

        await repo.AddAsync(pickup, ct);

        return pickup;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
