namespace NovaCore.Auth.Application.Features.Accounts.Commands.ReplaceAccountPermissions;

/// <summary>Replaces an Account's directly-granted (non-Role) permission set wholesale - same
/// "client sends the desired end-state, server diffs" convention as
/// UpdateRolePermissionsCommand.</summary>
public sealed record ReplaceAccountPermissionsCommand(Guid AccountId, IReadOnlyCollection<string> PermissionKeys) : ICommand;
