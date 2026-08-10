using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

using NovaCore.Order.Persistence.Engine;

namespace NovaCore.Order.Persistence.Contexts;

public abstract class OrderBaseRepository<TEntity>(OrderDbContext context)
    : GenericRepository<OrderDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}

public abstract class OrderBaseRepository<TEntity, TId>(OrderDbContext context)
    : EntityGenericRepository<OrderDbContext, TEntity, TId>(context)
    where TEntity : class, IEntity<TId>
{
}
