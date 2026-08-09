using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Write;

public sealed class PermissionWriteService(
    IPermissionDefinitionRepository repo,
    IUnitOfWork unitOfWork) : IPermissionWriteService, IPersistenceService
{
    public async Task UpdateAsync(Guid id, Action<PermissionDefinition> update, CancellationToken ct = default)
    {
        await repo.UpdateAsync(p => p.Id == id, update, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(p => p.Id == id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
