using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.Shipments.Repositories;

public sealed class ShipmentRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<Shipment, Guid>(dbContext), IShipmentRepository
{
    public async Task<Shipment?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
    {
        return await _dbContext.Shipments
            .AsNoTracking()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.IdempotencyKey == idempotencyKey, ct);
    }

    public async Task<Shipment?> GetByShipmentNumberAsync(string shipmentNumber, CancellationToken ct = default)
    {
        return await _dbContext.Shipments
            .AsNoTracking()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.ShipmentNumber.Value == shipmentNumber, ct);
    }
}
