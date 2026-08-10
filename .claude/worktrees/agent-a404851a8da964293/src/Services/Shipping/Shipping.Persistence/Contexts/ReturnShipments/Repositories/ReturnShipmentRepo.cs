using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.ReturnShipments.Repositories;

public sealed class ReturnShipmentRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<ReturnShipment, Guid>(dbContext), IReturnShipmentRepository
{
    public async Task<IReadOnlyList<ReturnShipment>> GetByOriginalShipmentIdAsync(Guid originalShipmentId, CancellationToken ct = default)
    {
        return await _dbContext.ReturnShipments
            .AsNoTracking()
            .Where(r => r.OriginalShipmentId == originalShipmentId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);
    }
}
