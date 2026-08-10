using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Contexts.Returns.Repositories;

public sealed class ReturnOrderRepo(OrderDbContext dbContext)
    : OrderBaseRepository<ReturnOrder, Guid>(dbContext), IReturnOrderRepository
{
}
