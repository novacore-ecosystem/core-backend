using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.Pickups.Repositories;

public interface IPickupRepository : IRepository<Pickup, Guid>
{
    Task<IReadOnlyList<Pickup>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken ct = default);
}
