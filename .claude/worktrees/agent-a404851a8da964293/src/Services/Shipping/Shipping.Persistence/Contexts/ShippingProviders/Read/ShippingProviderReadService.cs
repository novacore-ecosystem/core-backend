using NovaCore.Shipping.Application.Abstractions.Persistence.ShippingProviders;
using NovaCore.Shipping.Persistence.Contexts.ShippingProviders.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.ShippingProviders.Read;

public sealed class ShippingProviderReadService(IShippingProviderRepository repo) : IShippingProviderReadService
{
    public async Task<ShippingProvider?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, query => query.Include(p => p.Profile), ct);

    public async Task<ShippingProvider?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await repo.GetByCodeAsync(code, ct);
}
