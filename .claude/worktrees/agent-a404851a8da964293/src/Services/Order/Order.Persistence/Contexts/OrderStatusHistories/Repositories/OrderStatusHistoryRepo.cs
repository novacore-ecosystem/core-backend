using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Contexts.OrderStatusHistories.Repositories;

public sealed class OrderStatusHistoryRepo(OrderDbContext dbContext)
    : OrderBaseRepository<OrderStatusHistory, long>(dbContext), IOrderStatusHistoryRepository
{
}
