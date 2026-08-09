using NovaCore.Auth.Application.Features.Roles.DTOs;
using NovaCore.Auth.Domain.Entities.Roles;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Roles;

public interface IRoleWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - no caller-owned transaction exists yet.</summary>
    Task CreateAsync(Role role, CancellationToken ct = default);

    Task UpdateAsync(Guid id, Action<Role> update, CancellationToken ct = default);

    /// <summary>Replaces the Role's permission set wholesale with permissionKeys - loads current
    /// grants, resolves the requested PermissionDefinitions, diffs, and applies Assign/RemovePermission
    /// internally. Unknown keys are silently skipped (matches the prior handler-level behavior).</summary>
    Task<RolePermissionUpdateResult> UpdatePermissionsAsync(Guid id, IReadOnlyCollection<string> permissionKeys, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
