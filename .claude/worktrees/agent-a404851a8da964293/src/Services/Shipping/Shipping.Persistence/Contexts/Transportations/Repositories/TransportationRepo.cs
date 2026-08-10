using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.Transportations.Repositories;

public sealed class TransportationRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<Transportation, Guid>(dbContext), ITransportationRepository
{
    public async Task<Transportation?> GetByTransportationNumberAsync(string transportationNumber, CancellationToken ct = default)
    {
        return await _dbContext.Transportations
            .AsNoTracking()
            .Include(t => t.Assignment)
            .FirstOrDefaultAsync(t => t.TransportationNumber.Value == transportationNumber, ct);
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
