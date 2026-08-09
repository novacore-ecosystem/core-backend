namespace NovaCore.Auth.Application.Abstractions.Authorization;

/// <summary>
/// Resolves an Account's real, DB-backed effective permission set - the union of every permission
/// granted directly (AccountRole -> Role -> RolePermission) or through an effective Position
/// (AccountPosition -> Position -> PositionRole -> Role -> RolePermission), deduplicated. Replaces
/// RolePermissionMap's hard-coded switch (see docs/services/auth-service.md, Phase 3).
///
/// tenantId is always explicit, never read from RequestContext.Current: the primary caller is
/// Login, which resolves this before any tenant claim exists (see LoginHandler) - the same reason
/// Phase 2's Account/TenantClient lookups take an explicit tenantId instead of relying on ambient
/// context.
/// </summary>
public interface IEffectivePermissionReadService
{
    Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid accountId, Guid tenantId, CancellationToken ct = default);

    /// <summary>Every Account, within tenantId, currently holding roleId - directly (AccountRole)
    /// or through an effective Position (AccountPosition -> Position -> PositionRole). Used to
    /// find who needs their UserService permission projection recomputed after a RolePermission
    /// change (see docs/services/auth-service.md, Phase 3 - "Role-level change must propagate").</summary>
    Task<IReadOnlySet<Guid>> GetAccountIdsForRoleAsync(Guid roleId, Guid tenantId, CancellationToken ct = default);
}
