using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.Deliveries.Repositories;

public interface IDeliveryRepository : IRepository<Delivery, Guid>
{
    Task<Delivery?> GetByTransportationIdAsync(Guid transportationId, CancellationToken ct = default);
}
