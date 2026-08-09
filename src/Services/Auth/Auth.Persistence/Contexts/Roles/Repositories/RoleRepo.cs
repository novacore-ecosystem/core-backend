using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Persistence.Contexts;
using NovaCore.Auth.Persistence.Engine;

namespace NovaCore.Auth.Persistence.Contexts.Roles.Repositories;

public sealed class RoleRepo(AuthDbContext dbContext)
    : AuthBaseRepository<Role>(dbContext), IRoleRepository
{
}
