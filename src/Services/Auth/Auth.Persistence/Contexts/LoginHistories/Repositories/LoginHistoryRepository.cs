using NovaCore.Auth.Domain.Entities.Accounts;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.LoginHistories.Repositories;

public sealed class LoginHistoryRepository(AuthDbContext dbContext)
    : AuthBaseRepository<LoginHistory>(dbContext), ILoginHistoryRepository
{
}
