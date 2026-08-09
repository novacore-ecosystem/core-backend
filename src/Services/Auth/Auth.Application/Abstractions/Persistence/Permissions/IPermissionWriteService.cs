using NovaCore.Auth.Domain.Entities.Permissions;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Permissions;

public interface IPermissionWriteService
{
    Task UpdateAsync(Guid id, Action<PermissionDefinition> update, CancellationToken ct = default);

    /// <summary>Caller must guard IsSystemPermission before calling - root/user must never reach
    /// here (see docs/services/auth-service.md, Phase 3).</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
