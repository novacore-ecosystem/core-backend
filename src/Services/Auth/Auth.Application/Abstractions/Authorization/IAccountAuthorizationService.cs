namespace NovaCore.Auth.Application.Abstractions.Authorization;

/// <summary>
/// The Account authorization-management facade: replacing an account's roles or direct
/// permissions, and changing its management level.
/// </summary>
/// <remarks>
/// The single entry point Application command handlers call for these operations, instead of each
/// handler coordinating the guard, the effective-authorization cache, the Role/PermissionGrant
/// write paths and the effective-permission propagation event itself. Implemented in
/// Auth.Infrastructure (see docs/reference/caching.md's User Detail cache for the same
/// composition-in-Infrastructure shape) - it is authorization orchestration, not a persistence
/// concern, so it does not belong in Auth.Persistence.
/// </remarks>
public interface IAccountAuthorizationService
{
    /// <summary>
    /// Replaces an account's directly-assigned roles with the given set.
    /// </summary>
    /// <param name="actorId">The account performing the change.</param>
    /// <param name="accountId">The account being managed.</param>
    /// <param name="roleIds">The desired end-state role set.</param>
    /// <param name="tenantId">The tenant scope of the operation.</param>
    Task ReplaceRolesAsync(
        Guid actorId,
        Guid accountId,
        IReadOnlyCollection<Guid> roleIds,
        Guid tenantId,
        CancellationToken ct = default);

    /// <summary>
    /// Replaces an account's directly-granted permissions with the given set.
    /// </summary>
    /// <param name="actorId">The account performing the change.</param>
    /// <param name="accountId">The account being managed.</param>
    /// <param name="permissionKeys">The desired end-state permission set.</param>
    /// <param name="tenantId">The tenant scope of the operation.</param>
    Task ReplacePermissionsAsync(
        Guid actorId,
        Guid accountId,
        IReadOnlyCollection<string> permissionKeys,
        Guid tenantId,
        CancellationToken ct = default);

    /// <summary>
    /// Sets an account's management level.
    /// </summary>
    /// <param name="actorId">The account performing the change.</param>
    /// <param name="accountId">The account being managed.</param>
    /// <param name="level">The level to grant.</param>
    /// <param name="tenantId">The tenant scope of the operation.</param>
    Task SetLevelAsync(
        Guid actorId,
        Guid accountId,
        int level,
        Guid tenantId,
        CancellationToken ct = default);
}
