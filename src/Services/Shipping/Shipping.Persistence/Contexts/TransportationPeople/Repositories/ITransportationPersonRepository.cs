using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.TransportationPeople.Repositories;

public interface ITransportationPersonRepository : IRepository<TransportationPerson, Guid>
{
    Task<IReadOnlyList<TransportationPerson>> GetByProviderIdAsync(Guid providerId, CancellationToken ct = default);
}
