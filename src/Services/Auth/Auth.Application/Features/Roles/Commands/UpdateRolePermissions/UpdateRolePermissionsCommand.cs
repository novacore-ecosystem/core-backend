namespace NovaCore.Auth.Application.Features.Roles.Commands.UpdateRolePermissions;

/// <summary>Replaces the Role's permission set wholesale with PermissionKeys - the handler diffs
/// against the Role's current grants and applies Assign/Remove per key, rather than the caller
/// needing to know what's already granted.</summary>
public sealed record UpdateRolePermissionsCommand(Guid RoleId, IReadOnlyCollection<string> PermissionKeys) : ICommand;
