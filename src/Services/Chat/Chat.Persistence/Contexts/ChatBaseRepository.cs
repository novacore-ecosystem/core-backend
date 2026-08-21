using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

using NovaCore.Chat.Persistence.Engine;

namespace NovaCore.Chat.Persistence.Contexts;

public abstract class ChatBaseRepository<TEntity>(ChatDbContext context)
    : GenericRepository<ChatDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}

public abstract class ChatBaseRepository<TEntity, TId>(ChatDbContext context)
    : EntityGenericRepository<ChatDbContext, TEntity, TId>(context)
    where TEntity : class, IEntity<TId>
{
}
