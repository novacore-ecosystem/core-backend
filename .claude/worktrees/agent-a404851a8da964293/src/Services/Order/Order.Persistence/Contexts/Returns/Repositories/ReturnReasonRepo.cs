using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Contexts.Returns.Repositories;

public sealed class ReturnReasonRepo(OrderDbContext dbContext)
    : OrderBaseRepository<ReturnReason, Guid>(dbContext), IReturnReasonRepository
{
}
