using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.ShippingProviders.Repositories;

public sealed class ShippingProviderRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<ShippingProvider, Guid>(dbContext), IShippingProviderRepository
{
    public async Task<ShippingProvider?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _dbContext.ShippingProviders
            .AsNoTracking()
            .Include(p => p.Profile)
            .FirstOrDefaultAsync(p => p.Code == code.Trim().ToUpperInvariant(), ct);
    }
}
