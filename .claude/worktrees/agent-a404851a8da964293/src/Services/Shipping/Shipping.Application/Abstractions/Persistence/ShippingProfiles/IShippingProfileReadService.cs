namespace NovaCore.Shipping.Application.Abstractions.Persistence.ShippingProfiles;

public interface IShippingProfileReadService
{
    Task<ShippingProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ShippingProfile>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
