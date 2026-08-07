using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.Pickups.Repositories;

public sealed class PickupRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<Pickup, Guid>(dbContext), IPickupRepository
{
    public async Task<IReadOnlyList<Pickup>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken ct = default)
    {
        return await _dbContext.Pickups
            .AsNoTracking()
            .Where(p => p.ShipmentId == shipmentId)
            .OrderBy(p => p.ScheduledAt)
            .ToListAsync(ct);
    }
}
