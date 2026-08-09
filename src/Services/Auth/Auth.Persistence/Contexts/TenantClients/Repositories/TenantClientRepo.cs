using NovaCore.Auth.Domain.Entities.TenantClients;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.TenantClients.Repositories;

public sealed class TenantClientRepo(AuthDbContext dbContext)
    : AuthBaseRepository<TenantClient>(dbContext), ITenantClientRepository
{
}
