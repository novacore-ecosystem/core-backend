using NovaCore.Auth.Application.Features.Accounts.DTOs;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Accounts;

/// <summary>Direct (non-Position) Role assignment on an Account - the centralized replacement for
/// there being no dedicated admin path to Account.AssignRole/RemoveRole (see
/// docs/services/auth-service.md, Phase 3's "deferred gap" note).</summary>
public interface IAccountRoleAssignmentService
{
    /// <summary>Replaces the Account's directly-assigned Role set wholesale - loads the Account
    /// with its current AccountRoles, resolves the requested RoleIds (unknown ids are silently
    /// skipped, matching ReplaceForProviderAsync's documented behavior for permission keys), diffs,
    /// and applies Account.AssignRole/RemoveRole internally.</summary>
    Task<AccountRoleReplaceResult> ReplaceRolesAsync(
        Guid accountId,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken ct = default);
}
