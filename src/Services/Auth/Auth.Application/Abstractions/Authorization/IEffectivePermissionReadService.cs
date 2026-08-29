namespace NovaCore.Auth.Application.Abstractions.Authorization;

/// <summary>
/// Resolves an Account's real, DB-backed effective permission set - the union of every permission
/// granted directly (AccountRole -> Role -> PermissionGrant) or through an effective Position
/// (AccountPosition -> Position -> PositionRole -> Role -> PermissionGrant), deduplicated. Replaces
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

    /// <summary>Batched form of GetEffectivePermissionsAsync - one query for every accountId instead
    /// of one per account, since accounts can hold different Roles/Positions and therefore different
    /// effective permission sets (a single shared array cannot be assumed). An accountId with no
    /// resolvable permission is simply absent from the result - callers should treat a missing key
    /// as an empty set, not an error.</summary>
    Task<IReadOnlyDictionary<Guid, IReadOnlySet<string>>> GetEffectivePermissionsForAccountsAsync(IReadOnlyCollection<Guid> accountIds, Guid tenantId, CancellationToken ct = default);

    /// <summary>Every Account, within tenantId, currently holding roleId - directly (AccountRole)
    /// or through an effective Position (AccountPosition -> Position -> PositionRole). Used to
    /// find who needs their UserService permission projection recomputed after a PermissionGrant
    /// change (see docs/services/auth-service.md, Phase 3 - "Role-level change must propagate").</summary>
    Task<IReadOnlySet<Guid>> GetAccountIdsForRoleAsync(Guid roleId, Guid tenantId, CancellationToken ct = default);
}
