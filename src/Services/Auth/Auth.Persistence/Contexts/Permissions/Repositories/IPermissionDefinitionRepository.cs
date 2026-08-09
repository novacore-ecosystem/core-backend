using NovaCore.Auth.Domain.Entities.Permissions;

using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;

public interface IPermissionDefinitionRepository : IRepository<PermissionDefinition>
{
    // Leave empty for now - only generic CRUD is needed.
}
