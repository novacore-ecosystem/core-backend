namespace NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountRoles;

/// <summary>
/// Replaces an account's directly-assigned roles with the given set.
/// </summary>
/// <remarks>
/// Same "client sends the desired end-state, server diffs" convention as
/// UpdateRolePermissionsCommand.
/// </remarks>
public sealed record ReplaceAccountRolesCommand(Guid AccountId, IReadOnlyCollection<Guid> RoleIds) : ICommand;
