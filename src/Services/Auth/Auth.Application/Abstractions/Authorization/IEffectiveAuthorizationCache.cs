namespace NovaCore.Auth.Application.Abstractions.Authorization;

/// <summary>
/// Read-through cache for an Account's <see cref="AccountAuthorizationSnapshot"/>.
/// </summary>
/// <remarks>
/// Tenant-aware, short-TTL. Combines an Account's management Level with its effective permission
/// set into one cached unit, so repeated backend authorization checks in the same or later requests
/// don't each re-resolve the Role/Position/PermissionGrant join independently. Follows the User
/// Detail cache pattern (see docs/reference/caching.md) - explicit Infrastructure service called
/// directly by callers, not a decorator over an existing Read Service.
/// </remarks>
public interface IEffectiveAuthorizationCache
{
    /// <summary>
    /// Gets the account's authorization snapshot, populating the cache on a miss.
    /// </summary>
    /// <param name="accountId">The account to resolve.</param>
    /// <param name="tenantId">The tenant scope the snapshot is resolved within.</param>
    Task<AccountAuthorizationSnapshot> GetAsync(Guid accountId, Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Removes the cached snapshot for an account, e.g. after its roles/permissions/level change.
    /// </summary>
    /// <param name="accountId">The account whose snapshot is now stale.</param>
    /// <param name="tenantId">The tenant scope the snapshot was cached under.</param>
    Task InvalidateAsync(Guid accountId, Guid tenantId, CancellationToken ct = default);
}
