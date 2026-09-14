using NovaCore.Auth.Domain.Entities.Apps;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Apps.Repositories;

public sealed class AppRepo(AuthDbContext dbContext)
    : AuthBaseRepository<App>(dbContext), IAppRepository
{
}
