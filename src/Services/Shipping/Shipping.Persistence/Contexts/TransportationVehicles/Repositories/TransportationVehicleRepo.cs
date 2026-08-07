using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationVehicles.Repositories;

public sealed class TransportationVehicleRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<TransportationVehicle, Guid>(dbContext), ITransportationVehicleRepository
{
    public async Task<IReadOnlyList<TransportationVehicle>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default)
    {
        return await _dbContext.TransportationVehicles
            .AsNoTracking()
            .Where(v => v.ProviderId == providerId)
            .OrderBy(v => v.PlateNumber)
            .ToListAsync(ct);
    }
}
