using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.Application.Abstractions.Authorization;

/// <summary>
/// The authorization-scope rules an Account-targeting role/permission mutation must satisfy.
/// </summary>
/// <remarks>
/// Pure decision logic - no I/O. Callers (see <c>AccountAuthorizationService</c> in
/// Auth.Infrastructure) resolve both snapshots once per operation via
/// <see cref="IEffectiveAuthorizationCache"/> and reuse them across every check below, rather than
/// re-resolving effective permissions on each individual rule. An actor holding
/// <see cref="AccountAuthorizationSnapshot.HasRoot"/> bypasses every rule here; a target holding it
/// can never be managed - Root accounts are a provisioning/DB-seed-only concern.
/// </remarks>
public static class AccountAuthorizationGuard
{
    /// <summary>
    /// Ensures the actor outranks the target account.
    /// </summary>
    /// <param name="actor">The acting account's snapshot.</param>
    /// <param name="target">The account being managed.</param>
    public static void EnsureCanManageAccount(AccountAuthorizationSnapshot actor, AccountAuthorizationSnapshot target)
    {
        if (actor.HasRoot)
            return;

        if (target.HasRoot)
            throw new ForbiddenException("A Root account cannot be managed through this operation.");

        if (actor.Level <= target.Level)
            throw new ForbiddenException("You do not have sufficient authority to manage this account.");
    }

    /// <summary>
    /// Ensures the actor already holds every permission being granted.
    /// </summary>
    /// <param name="actor">The acting account's snapshot.</param>
    /// <param name="permissionKeys">The newly-added permission keys of a diff, not a full requested set.</param>
    public static void EnsureCanGrantPermissions(AccountAuthorizationSnapshot actor, IReadOnlyCollection<string> permissionKeys)
    {
        if (actor.HasRoot || permissionKeys.Count == 0)
            return;

        var missing = permissionKeys.Where(key => !actor.PermissionKeys.Contains(key)).ToArray();
        if (missing.Length > 0)
            throw new ForbiddenException($"You cannot grant a permission you do not hold: {string.Join(", ", missing)}.");
    }

    /// <summary>
    /// Ensures the actor is themselves assigned the role being granted.
    /// </summary>
    /// <param name="actor">The acting account's snapshot.</param>
    /// <param name="actorHoldsRole">Whether the actor is directly assigned the role being granted.</param>
    public static void EnsureCanGrantRole(AccountAuthorizationSnapshot actor, bool actorHoldsRole)
    {
        if (!actor.HasRoot && !actorHoldsRole)
            throw new ForbiddenException("You cannot assign a role you do not hold.");
    }

    /// <summary>
    /// Ensures a permission-bearing target that is not itself an Account (a Role today; a
    /// Position once it gets a direct grant path) cannot be modified once it grants Root, except
    /// by a Root-holding actor. Mirrors <see cref="EnsureCanManageAccount"/>'s Root-target
    /// protection for the Account path, where the target is a Role/Position's own permission set
    /// rather than an Account's.
    /// </summary>
    /// <param name="actor">The acting account's snapshot.</param>
    /// <param name="targetPermissionKeys">The target's permission keys before the mutation.</param>
    public static void EnsureTargetDoesNotGrantRoot(AccountAuthorizationSnapshot actor, IReadOnlySet<string> targetPermissionKeys)
    {
        if (actor.HasRoot)
            return;

        if (targetPermissionKeys.Contains(Permissions.Root))
            throw new ForbiddenException("A Root-granting Role cannot be managed through this operation.");
    }

    /// <summary>
    /// Ensures every newly-added permission key falls within the tenant's configured permission
    /// boundary - the subset of the catalog ROOT has allowed this tenant's own Role/User grants to
    /// draw from (<see cref="PermissionProviderName.Tenant"/> grants). A Root-holding actor
    /// bypasses this - ROOT provisions the boundary itself and is not bound by it. A tenant that
    /// has not opted into boundary enforcement (<paramref name="boundaryEnabled"/> false) stays
    /// fully unrestricted, so this is backward-compatible with every tenant that predates the
    /// feature.
    /// </summary>
    /// <param name="actor">The acting account's snapshot.</param>
    /// <param name="boundaryEnabled">Whether the target tenant has opted into boundary enforcement.</param>
    /// <param name="tenantAllowedKeys">The permission keys ROOT has granted to the tenant.</param>
    /// <param name="newlyAddedKeys">The newly-added permission keys of a diff, not a full requested set.</param>
    public static void EnsureWithinTenantBoundary(
        AccountAuthorizationSnapshot actor,
        bool boundaryEnabled,
        IReadOnlySet<string> tenantAllowedKeys,
        IReadOnlyCollection<string> newlyAddedKeys)
    {
        if (actor.HasRoot || !boundaryEnabled || newlyAddedKeys.Count == 0)
            return;

        var outOfBounds = newlyAddedKeys.Where(key => !tenantAllowedKeys.Contains(key)).ToArray();
        if (outOfBounds.Length > 0)
            throw new ForbiddenException(
                $"This tenant is not permitted to grant: {string.Join(", ", outOfBounds)}.");
    }
}
