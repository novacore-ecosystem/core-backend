namespace NovaCore.Auth.Application.Features.Tenants.Commands.AssignTenantPermissions;

/// <summary>Explicit grant/revoke delta, not a full-replace - matches the same
/// AssignmentMutation {grant, revoke} contract PermissionAssignmentService uses for Role/User
/// grants (see docs/services/auth-service.md), so the frontend's shared Access Control module
/// talks to this endpoint exactly the same way it talks to the Account/Role ones.</summary>
public sealed record AssignTenantPermissionsCommand(
    Guid TenantId,
    IReadOnlyCollection<string> Grant,
    IReadOnlyCollection<string> Revoke) : ICommand;
