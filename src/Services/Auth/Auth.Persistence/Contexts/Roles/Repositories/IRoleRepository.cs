using NovaCore.Auth.Domain.Entities.Roles;

using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Roles.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    // Leave empty for now - only generic CRUD is needed.
}
