using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.VerifiedShippingAddresses.Repositories;

public sealed class VerifiedShippingAddressRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<VerifiedShippingAddress, Guid>(dbContext), IVerifiedShippingAddressRepository
{
    public async Task<IReadOnlyList<VerifiedShippingAddress>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.VerifiedShippingAddresses
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.SuccessfulDeliveryCount)
            .ToListAsync(ct);
    }
}
