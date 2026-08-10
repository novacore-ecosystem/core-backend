using NovaCore.Shipping.Application.Abstractions.Persistence.VerifiedShippingAddresses;
using NovaCore.Shipping.Persistence.Contexts.VerifiedShippingAddresses.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.VerifiedShippingAddresses.Read;

public sealed class VerifiedShippingAddressReadService(IVerifiedShippingAddressRepository repo) : IVerifiedShippingAddressReadService
{
    public async Task<VerifiedShippingAddress?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, ct);

    public async Task<IReadOnlyList<VerifiedShippingAddress>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await repo.GetByUserIdAsync(userId, ct);
}
