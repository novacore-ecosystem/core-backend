using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationVehicles.Repositories;

public interface ITransportationVehicleRepository : IRepository<TransportationVehicle, Guid>
{
    Task<IReadOnlyList<TransportationVehicle>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default);
}
