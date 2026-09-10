namespace NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountRoles;

/// <summary>Replaces an Account's directly-assigned Role set wholesale - same "client sends the
/// desired end-state, server diffs" convention as UpdateRolePermissionsCommand.</summary>
public sealed record ReplaceAccountRolesCommand(Guid AccountId, IReadOnlyCollection<Guid> RoleIds) : ICommand;
