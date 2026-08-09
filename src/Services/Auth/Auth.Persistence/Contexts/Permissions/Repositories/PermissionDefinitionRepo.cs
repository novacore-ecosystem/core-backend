using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;

public sealed class PermissionDefinitionRepo(AuthDbContext dbContext)
    : AuthBaseRepository<PermissionDefinition>(dbContext), IPermissionDefinitionRepository
{
}
