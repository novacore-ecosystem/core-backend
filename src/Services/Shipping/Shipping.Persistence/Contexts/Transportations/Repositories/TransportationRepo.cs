using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.Transportations.Repositories;

public sealed class TransportationRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<Transportation, Guid>(dbContext), ITransportationRepository
{
    public async Task<Transportation?> GetByTransportationNumberAsync(string transportationNumber, CancellationToken ct = default)
    {
        // Compare the value object itself, not t.TransportationNumber.Value - TransportationConfig
        // maps TransportationNumber via HasConversion, which EF can translate for an Equal on the
        // mapped property directly, but not for an arbitrary member access (.Value) inside the
        // expression tree (throws "could not be translated" at query time).
        var number = TransportationNumber.Create(transportationNumber);
        return await _dbContext.Transportations
            .AsNoTracking()
            .Include(t => t.Assignment)
            .FirstOrDefaultAsync(t => t.TransportationNumber == number, ct);
    }

    public async Task<IReadOnlyList<Transportation>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken ct = default)
    {
        return await _dbContext.Transportations
            .AsNoTracking()
            .Include(t => t.Assignment)
            .Where(t => t.ShipmentId == shipmentId)
            .OrderBy(t => t.AttemptNo)
            .ToListAsync(ct);
    }
}
