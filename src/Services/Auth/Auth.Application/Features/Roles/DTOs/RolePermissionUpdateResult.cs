namespace NovaCore.Auth.Application.Features.Roles.DTOs;

/// <summary>Outcome of RoleWriteService.UpdatePermissionsAsync - lets Application decide whether to
/// propagate an authorization change without inspecting Role.Permissions itself. PermissionKeys is
/// the Role's resulting permission set, not any Account's full effective set (an Account can hold
/// this Role plus others) - callers needing effective permissions still resolve those separately.</summary>
public sealed record RolePermissionUpdateResult(bool HasChanges, IReadOnlyList<string> PermissionKeys);
