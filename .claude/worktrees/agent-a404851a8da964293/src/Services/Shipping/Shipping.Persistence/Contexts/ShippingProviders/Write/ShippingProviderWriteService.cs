using NovaCore.Shipping.Application.Abstractions.Persistence.ShippingProviders;
using NovaCore.Shipping.Persistence.Contexts.ShippingProviders.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.ShippingProviders.Write;

public sealed class ShippingProviderWriteService(IShippingProviderRepository repo) : IShippingProviderWriteService
{
    public async Task<ShippingProvider> CreateAsync(
        string code,
        string name,
        ProviderType providerType,
        CancellationToken ct = default)
    {
        var provider = ShippingProvider.Create(code, name, providerType);

        await repo.AddAsync(provider, ct);

        return provider;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        => await repo.DeleteByIdAsync(id, ct);
}
