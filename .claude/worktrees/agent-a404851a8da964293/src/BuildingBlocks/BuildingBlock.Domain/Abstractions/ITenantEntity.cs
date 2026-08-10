namespace NovaCore.BuildingBlock.Domain.Abstractions;

/// <summary>
/// Opt-in capability for entities owned by exactly one Tenant. Implementing this interface is
/// what makes an entity participate in the Entity Convention scan (see
/// NovaCore.BuildingBlock.Persistence.Ef) - TenantId mapping, indexing, and query-filtering are
/// then applied automatically, with no per-entity EF configuration required. Business code never
/// assigns TenantId directly - see AssignTenant.
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; }

    /// <summary>Framework-only assignment point (TenantAssignmentInterceptor) - must be idempotent
    /// (no-op once already assigned), so an entity that needs an explicit tenant at construction
    /// time (e.g. Scope) is never overwritten by the interceptor's default assignment.</summary>
    void AssignTenant(Guid tenantId);
}
