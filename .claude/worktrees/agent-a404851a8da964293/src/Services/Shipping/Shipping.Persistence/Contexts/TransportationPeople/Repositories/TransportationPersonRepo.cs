using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationPeople.Repositories;

public sealed class TransportationPersonRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<TransportationPerson, Guid>(dbContext), ITransportationPersonRepository
{
    public async Task<IReadOnlyList<TransportationPerson>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default)
    {
        return await _dbContext.TransportationPeople
            .AsNoTracking()
            .Where(p => p.ProviderId == providerId)
            .OrderBy(p => p.FullName)
            .ToListAsync(ct);
    }
}
