using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Order.Persistence.Contexts.OrderStatusHistories.Repositories;

public interface IOrderStatusHistoryRepository : IRepository<OrderStatusHistory, long>
{
}
