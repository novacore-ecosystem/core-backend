using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

using NovaCore.User.Persistence.Engine;

namespace NovaCore.User.Persistence.Contexts;

public abstract class UserBaseRepository<TEntity>(UserDbContext context)
    : GenericRepository<UserDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}
