using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.CarrierIntegrations.Repositories;

public interface ICarrierIntegrationRepository : IRepository<CarrierIntegration, Guid>
{
    Task<CarrierIntegration?> GetByProviderIdAsync(Guid shippingProviderId, CancellationToken ct = default);
}
