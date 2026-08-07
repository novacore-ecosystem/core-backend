using NovaCore.Shipping.Application.Abstractions.Persistence.ShippingProfiles;
using NovaCore.Shipping.Persistence.Contexts.ShippingProfiles.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.ShippingProfiles.Write;

public sealed class ShippingProfileWriteService(IShippingProfileRepository repo) : IShippingProfileWriteService
{
    public async Task<ShippingProfile> CreateAsync(
        Guid userId,
        string label,
        string contactName,
        PhoneNumber contactPhone,
        ShippingAddress address,
        bool isDefault = false,
        CancellationToken ct = default)
    {
        var profile = ShippingProfile.Create(userId, label, contactName, contactPhone, address, isDefault);

        await repo.AddAsync(profile, ct);

        return profile;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
