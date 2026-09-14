using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.BuildingBlock.Persistence.Repository;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;

public interface IPermissionGrantRepository : IRepository<PermissionGrant>
{
    // The four methods below all read across the explicit tenantId given, not the ambient
    // RequestContext (IgnoreQueryFilters) - PermissionGrantService is called from contexts (seed
    // provisioning, pre-auth resolution) where the ambient tenant filter would be wrong or unset.
    // None of this is expressible via the generic Get/Exists overloads (compound predicates need
    // more than one selector=value equality, and IgnoreQueryFilters isn't exposed generically).

    Task<bool> ExistsForProviderAsync(
        Guid tenantId,
        Guid permissionDefinitionId,
        PermissionProviderName providerName,
        string providerKey,
        CancellationToken ct = default);

    Task<PermissionGrant?> GetForProviderAsync(
        Guid tenantId,
        string permissionKey,
        PermissionProviderName providerName,
        string providerKey,
        CancellationToken ct = default);

    Task<List<PermissionGrant>> ListForProviderAsync(
        Guid tenantId,
        PermissionProviderName providerName,
        string providerKey,
        CancellationToken ct = default);

    Task<IReadOnlySet<string>> GetGrantedKeysAsync(
        Guid tenantId,
        PermissionProviderName providerName,
        string providerKey,
        CancellationToken ct = default);

    /// <summary>Ambient-tenant-filtered (respects RequestContext, unlike the four methods above) -
    /// for callers operating inside an authenticated request, e.g. RoleReadService.</summary>
    Task<IReadOnlyList<string>> GetKeysByProviderAsync(
        PermissionProviderName providerName,
        string providerKey,
        CancellationToken ct = default);

    /// <summary>Stages already-fetched grants for removal (no extra per-entity query, unlike
    /// DeleteAsync(predicate)) - the caller still owns SaveChangesAsync via IUnitOfWork.</summary>
    Task RemoveRangeAsync(IEnumerable<PermissionGrant> grants, CancellationToken ct = default);
}
