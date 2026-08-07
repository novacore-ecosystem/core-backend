using NovaCore.Shipping.Application.Abstractions.Persistence.CarrierIntegrations;
using NovaCore.Shipping.Persistence.Contexts.CarrierIntegrations.Repositories;

namespace NovaCore.Shipping.Persistence.Contexts.CarrierIntegrations.Read;

public sealed class CarrierIntegrationReadService(ICarrierIntegrationRepository repo) : ICarrierIntegrationReadService
{
    public async Task<CarrierIntegration?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await repo.GetByIdAsync(id, ct);

    public async Task<CarrierIntegration?> GetByProviderIdAsync(Guid shippingProviderId, CancellationToken ct = default)
        => await repo.GetByProviderIdAsync(shippingProviderId, ct);
}
