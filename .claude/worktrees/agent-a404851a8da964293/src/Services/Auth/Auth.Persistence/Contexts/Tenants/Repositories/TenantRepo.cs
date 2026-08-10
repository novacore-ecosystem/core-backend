using NovaCore.Auth.Domain.Entities.Tenants;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Tenants.Repositories;

public sealed class TenantRepo(AuthDbContext dbContext)
    : AuthBaseRepository<Tenant>(dbContext), ITenantRepository
{
}
