using NovaCore.Auth.Application.Features.Roles.DTOs;
using NovaCore.Auth.Domain.Entities.Roles;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Roles;

public interface IRoleWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - no caller-owned transaction exists yet.</summary>
    Task CreateAsync(Role role, CancellationToken ct = default);

    Task UpdateAsync(Guid id, Action<Role> update, CancellationToken ct = default);

    /// <summary>Replaces the Role's permission set wholesale with permissionKeys, via the
    /// centralized PermissionGrant table (ProviderName = Role, ProviderKey = id) - loads current
    /// grants, resolves the requested PermissionDefinitions, diffs, and applies grant/revoke
    /// internally. Unknown keys are silently skipped (matches the prior handler-level behavior); a
    /// known key not allowed for the Role provider throws.</summary>
    Task<RolePermissionUpdateResult> UpdatePermissionsAsync(Guid id, IReadOnlyCollection<string> permissionKeys, Guid tenantId, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
