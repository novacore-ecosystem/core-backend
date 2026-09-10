namespace NovaCore.Auth.Application.Abstractions.Authorization;

/// <summary>
/// Server-side enforcement of the two authorization-scope rules every Account-targeting
/// role/permission mutation must satisfy: an actor can only manage an Account they outrank
/// (Account.Level), and can only grant a Role/Permission they themselves already hold. Always
/// re-derives the actor's and target's current state fresh per call - never trusts a
/// client-submitted scope - the same principle LoginHandler/UpdateRolePermissionsHandler already
/// apply to effective-permission resolution.
///
/// An actor holding Permissions.Root bypasses every check here (same unconditional bypass
/// PermissionAuthorization already applies everywhere else); a target holding Permissions.Root can
/// never be managed through these checks, regardless of the actor's own Level - Root accounts are a
/// provisioning/DB-seed-only concern (see Permissions.Common.cs).
/// </summary>
public interface IAccountAuthorizationGuard
{
    /// <summary>Throws ForbiddenException unless the actor outranks the target Account
    /// (actor.Level > target.Level), or the actor holds Permissions.Root.</summary>
    Task EnsureCanManageAccountAsync(Guid actorId, Guid targetAccountId, Guid tenantId, CancellationToken ct = default);

    /// <summary>Throws ForbiddenException if the actor is missing any of the given permission keys
    /// from their own effective permission set, unless the actor holds Permissions.Root. Callers
    /// should pass only the newly-added keys of a diff, not a full requested set - see
    /// IPermissionGrantService.GetGrantedKeysAsync.</summary>
    Task EnsureCanGrantPermissionsAsync(Guid actorId, IReadOnlyCollection<string> permissionKeys, Guid tenantId, CancellationToken ct = default);

    /// <summary>Throws ForbiddenException unless the actor is themselves directly assigned roleId,
    /// or holds Permissions.Root.</summary>
    Task EnsureCanGrantRoleAsync(Guid actorId, Guid roleId, Guid tenantId, CancellationToken ct = default);
}
