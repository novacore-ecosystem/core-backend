using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Contexts.Returns.Repositories;

public sealed class ReturnStatusHistoryRepo(OrderDbContext dbContext)
    : OrderBaseRepository<ReturnStatusHistory, long>(dbContext), IReturnStatusHistoryRepository
{
}
