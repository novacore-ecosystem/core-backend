namespace NovaCore.Shipping.Application.Abstractions.Persistence.VerifiedShippingAddresses;

public interface IVerifiedShippingAddressWriteService
{

    Task<VerifiedShippingAddress> CreateAsync(
        Guid userId,
        ShippingAddress address,
        GeoCoordinate? coordinate = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
