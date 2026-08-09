namespace NovaCore.Auth.Application.Features.Permissions.Commands.UpdatePermission;

/// <summary>Regroups a permission - the only mutable field a PermissionDefinition has (Key is
/// immutable). Allowed on system permissions too (root/user "can be updated, cannot be deleted" -
/// see docs/services/auth-service.md, Phase 3).</summary>
public sealed record UpdatePermissionCommand(Guid Id, Guid PermissionGroupId) : ICommand;
