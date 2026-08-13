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
        // Compare the value object itself, not s.ShipmentNumber.Value - ShipmentConfig maps
        // ShipmentNumber via HasConversion, which EF can translate for an Equal on the mapped
        // property directly, but not for an arbitrary member access (.Value) inside the
        // expression tree (throws "could not be translated" at query time).
        var number = ShipmentNumber.Create(shipmentNumber);
        return await _dbContext.Shipments
            .AsNoTracking()
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.ShipmentNumber == number, ct);
    }
}
