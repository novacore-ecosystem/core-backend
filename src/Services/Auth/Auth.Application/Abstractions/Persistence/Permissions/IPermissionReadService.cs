using NovaCore.Auth.Domain.Entities.Permissions;

namespace NovaCore.Auth.Application.Abstractions.Persistence.Permissions;

public interface IPermissionReadService
{
    Task<PermissionDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<PermissionDefinition>> ListAsync(CancellationToken ct = default);
}
