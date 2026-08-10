using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

namespace NovaCore.Auth.Persistence.Contexts;

public abstract class AuthBaseRepository<TEntity>(AuthDbContext context)
    : GenericRepository<AuthDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}
