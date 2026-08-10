using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.ShippingProfiles.Repositories;

public sealed class ShippingProfileRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<ShippingProfile, Guid>(dbContext), IShippingProfileRepository
{
    public async Task<IReadOnlyList<ShippingProfile>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.ShippingProfiles
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.IsDefault)
            .ThenByDescending(p => p.LastUsedAt)
            .ToListAsync(ct);
    }
}
