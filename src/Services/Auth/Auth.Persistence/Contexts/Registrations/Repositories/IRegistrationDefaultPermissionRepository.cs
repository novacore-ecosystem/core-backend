using NovaCore.Auth.Domain.Entities.Registrations;

using NovaCore.BuildingBlock.Persistence.Repository;

namespace NovaCore.Auth.Persistence.Contexts.Registrations.Repositories;

public interface IRegistrationDefaultPermissionRepository : IRepository<RegistrationDefaultPermission>
{
    // Both methods read across the explicit (tenantId, appId) given, not the ambient
    // RequestContext (IgnoreQueryFilters) - RegisterHandler resolves this before any tenant
    // context is necessarily established. Not expressible via the generic Get/GetMany overloads.

    Task<IReadOnlyCollection<string>> GetPermissionKeysAsync(Guid tenantId, Guid appId, CancellationToken ct = default);

    Task<List<RegistrationDefaultPermission>> ListForAppAsync(Guid tenantId, Guid appId, CancellationToken ct = default);

    /// <summary>Stages already-fetched rows for removal (no extra per-entity query, unlike
    /// DeleteAsync(predicate)) - the caller still owns SaveChangesAsync via IUnitOfWork.</summary>
    Task RemoveRangeAsync(IEnumerable<RegistrationDefaultPermission> entities, CancellationToken ct = default);
}
