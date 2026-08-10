namespace NovaCore.Shipping.Application.Abstractions.Persistence.ShippingProfiles;

public interface IShippingProfileWriteService
{

    Task<ShippingProfile> CreateAsync(
        Guid userId,
        string label,
        string contactName,
        PhoneNumber contactPhone,
        ShippingAddress address,
        bool isDefault = false,
        CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
