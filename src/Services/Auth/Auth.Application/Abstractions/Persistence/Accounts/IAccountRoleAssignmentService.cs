using NovaCore.Auth.Application.Features.Accounts.DTOs;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

/// <summary>
/// Direct (non-Position) Role assignment on an Account.
/// </summary>
public interface IAccountRoleAssignmentService
{
    /// <summary>
    /// Replaces the account's directly-assigned roles with the given set.
    /// </summary>
    /// <remarks>
    /// Diffs against the account's current AccountRoles and applies Account.AssignRole/RemoveRole
    /// internally; an unknown RoleId is silently skipped, matching
    /// IPermissionGrantService.ReplaceForProviderAsync's behavior for permission keys.
    /// </remarks>
    /// <param name="accountId">The account being managed.</param>
    /// <param name="roleIds">The desired end-state role set.</param>
    Task<AccountRoleReplaceResult> ReplaceRolesAsync(
        Guid accountId,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken ct = default);

    /// <summary>
    /// Removes one directly-assigned Role from the account, leaving every other role untouched -
    /// unlike ReplaceRolesAsync, which requires the full desired role set. Idempotent - a missing
    /// assignment is a no-op, not an error.
    /// </summary>
    /// <param name="accountId">The account being managed.</param>
    /// <param name="roleId">The Role to remove.</param>
    Task RemoveRoleAsync(Guid accountId, Guid roleId, CancellationToken ct = default);

    /// <summary>
    /// Every AccountID currently directly assigned the given Role, in one batch query - e.g. used
    /// to find and reconcile stale Role assignments (Root account provisioning's singleton check).
    /// </summary>
    /// <param name="roleId">The Role to look up.</param>
    Task<IReadOnlyCollection<Guid>> GetAccountIdsInRoleAsync(Guid roleId, CancellationToken ct = default);
}
