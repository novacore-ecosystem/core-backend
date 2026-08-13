using NovaCore.BuildingBlock.Persistence.Ef.Repository;

using NovaCore.User.Persistence.Engine;

namespace NovaCore.User.Persistence.Contexts.Users.Repositories;

// Uses EntityGenericRepository directly (not UserBaseRepository) because UserWriteService needs
// the by-id UpdateAsync(Guid id, ...)/DeleteByIdAsync overloads, which only exist on
// IRepository<TEntity, TId> - UserBaseRepository<TEntity> only provides the predicate-based
// IRepository<TEntity> shape.
public sealed class UserRepo(UserDbContext dbContext)
    : EntityGenericRepository<UserDbContext, UserEntity, Guid>(dbContext), IUserRepository
{
}
