using NovaCore.User.Persistence.Engine;

namespace NovaCore.User.Persistence.Contexts.Users.Repositories;

public sealed class UserRepo(UserDbContext dbContext)
    : UserBaseRepository<UserEntity>(dbContext), IUserRepository
{
}
