using NovaCore.Auth.Domain.Entities.Scopes;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Scopes.Repositories;

public sealed class ScopeRepository(AuthDbContext dbContext)
    : AuthBaseRepository<Scope>(dbContext), IScopeRepository
{
}
