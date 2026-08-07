namespace NovaCore.Shipping.Application.Abstractions.Persistence.CarrierIntegrations;

public interface ICarrierIntegrationReadService
{
    Task<CarrierIntegration?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<CarrierIntegration?> GetByProviderIdAsync(Guid shippingProviderId, CancellationToken ct = default);
}
