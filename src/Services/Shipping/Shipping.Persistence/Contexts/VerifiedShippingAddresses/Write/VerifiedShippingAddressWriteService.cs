using NovaCore.Shipping.Application.Abstractions.Persistence.VerifiedShippingAddresses;
using NovaCore.Shipping.Persistence.Contexts.VerifiedShippingAddresses.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.VerifiedShippingAddresses.Write;

public sealed class VerifiedShippingAddressWriteService(IVerifiedShippingAddressRepository repo) : IVerifiedShippingAddressWriteService
{
    public async Task<VerifiedShippingAddress> CreateAsync(
        Guid userId,
        ShippingAddress address,
        GeoCoordinate? coordinate = null,
        CancellationToken ct = default)
    {
        var verifiedAddress = VerifiedShippingAddress.Create(userId, address, coordinate);

        await repo.AddAsync(verifiedAddress, ct);

        return verifiedAddress;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
