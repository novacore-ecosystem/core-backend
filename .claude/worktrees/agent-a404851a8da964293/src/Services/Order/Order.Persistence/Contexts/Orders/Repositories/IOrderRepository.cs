using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Order.Persistence.Contexts.Orders.Repositories;

public interface IOrderRepository : IRepository<OrderEntity, Guid>
{
    Task<OrderEntity?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken ct = default);
}
