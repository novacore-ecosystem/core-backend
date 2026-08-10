using NovaCore.Shipping.Application.Abstractions.Persistence.ShippingProfiles;
using NovaCore.Shipping.Persistence.Contexts.ShippingProfiles.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.ShippingProfiles.Read;

public sealed class ShippingProfileReadService(IShippingProfileRepository repo) : IShippingProfileReadService
{
    public async Task<ShippingProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<ShippingProfile>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await repo.GetByUserIdAsync(userId, ct);
}
