namespace NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountPermissions;

/// <summary>
/// Replaces an account's directly-granted (non-Role) permissions with the given set.
/// </summary>
/// <remarks>
/// Same "client sends the desired end-state, server diffs" convention as
/// UpdateRolePermissionsCommand.
/// </remarks>
public sealed record ReplaceAccountPermissionsCommand(
    Guid AccountId,
    IReadOnlyCollection<string> PermissionKeys) : ICommand;
