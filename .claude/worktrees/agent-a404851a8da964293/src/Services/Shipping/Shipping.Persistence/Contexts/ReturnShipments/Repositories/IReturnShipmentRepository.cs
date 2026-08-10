using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Shipping.Persistence.Contexts.ReturnShipments.Repositories;

public interface IReturnShipmentRepository : IRepository<ReturnShipment, Guid>
{
    Task<IReadOnlyList<ReturnShipment>> GetByOriginalShipmentIdAsync(Guid originalShipmentId, CancellationToken ct = default);
}
