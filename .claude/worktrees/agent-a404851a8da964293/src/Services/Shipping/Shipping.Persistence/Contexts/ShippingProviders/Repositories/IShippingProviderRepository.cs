using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.ShippingProviders.Repositories;

public interface IShippingProviderRepository : IRepository<ShippingProvider, Guid>
{
    Task<ShippingProvider?> GetByCodeAsync(string code, CancellationToken ct = default);
}
