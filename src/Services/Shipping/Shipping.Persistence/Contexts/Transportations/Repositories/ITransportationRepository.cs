using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.Transportations.Repositories;

public interface ITransportationRepository : IRepository<Transportation, Guid>
{
    Task<Transportation?> GetByTransportationNumberAsync(string transportationNumber, CancellationToken ct = default);

    Task<IReadOnlyList<Transportation>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken ct = default);
}
