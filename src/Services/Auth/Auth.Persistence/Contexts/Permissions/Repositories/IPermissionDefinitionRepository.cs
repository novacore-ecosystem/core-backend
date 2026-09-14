using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;

public interface IPermissionDefinitionRepository : IRepository<PermissionDefinition>
{
    /// <summary>Every PermissionDefinition with its PermissionGroup, ordered by group sort order
    /// then key - not expressible via the generic Get/GetMany overloads (ordering isn't
    /// supported generically), so it lives here as a custom method.</summary>
    Task<IReadOnlyList<PermissionDefinition>> ListAsync(CancellationToken ct = default);
}
