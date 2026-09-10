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
}
