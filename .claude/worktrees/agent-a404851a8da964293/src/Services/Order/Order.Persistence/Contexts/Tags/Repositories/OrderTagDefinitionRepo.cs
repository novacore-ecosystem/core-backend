using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Contexts.Tags.Repositories;

public sealed class OrderTagDefinitionRepo(OrderDbContext dbContext)
    : OrderBaseRepository<OrderTagDefinition, Guid>(dbContext), IOrderTagDefinitionRepository
{
}
