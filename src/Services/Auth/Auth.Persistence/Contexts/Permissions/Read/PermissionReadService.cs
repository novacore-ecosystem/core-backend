using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Read;

public sealed class PermissionReadService(IPermissionDefinitionRepository permissionDefinitionRepo)
    : IPermissionReadService, IPersistenceService
{
    public async Task<PermissionDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await permissionDefinitionRepo.GetAsync(
            p => p.Id == id,
            q => q.Include(p => p.PermissionGroup),
            ct);
    }

    public async Task<IReadOnlyList<PermissionDefinition>> ListAsync(CancellationToken ct = default)
    {
        return await permissionDefinitionRepo.ListAsync(ct);
    }
}
