namespace NovaCore.Shipping.Application.Abstractions.Persistence.VerifiedShippingAddresses;

public interface IVerifiedShippingAddressReadService
{
    Task<VerifiedShippingAddress?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<VerifiedShippingAddress>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
