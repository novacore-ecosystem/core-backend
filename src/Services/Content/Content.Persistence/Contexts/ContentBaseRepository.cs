using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Contexts;

public abstract class ContentBaseRepository<TEntity>(ContentDbContext context)
    : GenericRepository<ContentDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}

public abstract class ContentBaseRepository<TEntity, TId>(ContentDbContext context)
    : EntityGenericRepository<ContentDbContext, TEntity, TId>(context)
    where TEntity : class, IEntity<TId>
{
}
