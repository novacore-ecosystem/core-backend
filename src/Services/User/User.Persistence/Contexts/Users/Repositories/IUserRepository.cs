using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.User.Persistence.Contexts.Users.Repositories;

// Reserved for future scaling (e.g. a bulk lookup keyed by something other than Id) - nothing
// beyond the by-id CRUD IRepository<UserEntity, Guid> already provides is needed yet.
public interface IUserRepository : IRepository<UserEntity, Guid>
{
}
