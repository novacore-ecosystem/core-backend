using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Application.Features.Roles.DTOs;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Persistence.Contexts.Roles.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Persistence.Contexts.Roles.Write;

public sealed class RoleWriteService(
    IRoleRepository repo,
    IPermissionGrantService permissionGrantService,
    IUnitOfWork unitOfWork) : IRoleWriteService, IPersistenceService
{
    public async Task CreateAsync(Role role, CancellationToken ct = default)
    {
        await repo.AddAsync(role, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Guid id, Action<Role> update, CancellationToken ct = default)
    {
        await repo.UpdateAsync(r => r.Id == id, update, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>Replaces the Role's permission set wholesale via the centralized PermissionGrant
    /// table (ProviderName = Role, ProviderKey = this Role's Id) - Role no longer owns a
    /// permission-grant collection itself, see Role's class doc comment.</summary>
    public async Task<RolePermissionUpdateResult> UpdatePermissionsAsync(
        Guid id,
        IReadOnlyCollection<string> permissionKeys,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var result = await permissionGrantService.ReplaceForProviderAsync(
            PermissionProviderName.Role,
            id.ToString(),
            permissionKeys,
            tenantId,
            ct);

        return new RolePermissionUpdateResult(result.HasChanges, [.. result.ResultingKeys]);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(r => r.Id == id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
