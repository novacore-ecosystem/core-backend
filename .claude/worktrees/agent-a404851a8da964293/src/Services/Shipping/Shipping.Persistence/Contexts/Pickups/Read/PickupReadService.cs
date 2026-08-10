using NovaCore.Shipping.Application.Abstractions.Persistence.Pickups;
using NovaCore.Shipping.Persistence.Contexts.Pickups.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.Pickups.Read;

public sealed class PickupReadService(IPickupRepository repo) : IPickupReadService
{
    public async Task<Pickup?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<Pickup>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken ct = default)
        => await repo.GetByShipmentIdAsync(shipmentId, ct);
}
