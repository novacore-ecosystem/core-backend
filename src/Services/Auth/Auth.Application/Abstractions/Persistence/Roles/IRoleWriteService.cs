using NovaCore.Auth.Domain.Entities.Roles;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Roles;

public interface IRoleWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - no caller-owned transaction exists yet.</summary>
    Task CreateAsync(Role role, CancellationToken ct = default);

    Task UpdateAsync(Guid id, Action<Role> update, CancellationToken ct = default);

    /// <summary>Loads Role.Permissions before invoking update, since permission assignment reads/
    /// mutates that collection (see UpdateRolePermissionsHandler).</summary>
    Task UpdateWithPermissionsAsync(Guid id, Action<Role> update, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
