using NovaCore.Shipping.Persistence.Engine;

namespace NovaCore.Shipping.Persistence.Contexts.CarrierIntegrations.Repositories;

public sealed class CarrierIntegrationRepo(ShippingDbContext dbContext)
    : ShippingBaseRepository<CarrierIntegration, Guid>(dbContext), ICarrierIntegrationRepository
{
    public async Task<CarrierIntegration?> GetByProviderIdAsync(Guid shippingProviderId, CancellationToken ct = default)
        => await GetAsync(x => x.ShippingProviderId, shippingProviderId, ct);
}
