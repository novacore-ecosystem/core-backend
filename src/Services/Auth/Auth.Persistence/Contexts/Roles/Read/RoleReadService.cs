using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;
using NovaCore.Auth.Persistence.Contexts.Roles.Repositories;
using NovaCore.BuildingBlock.Persistence;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Persistence.Contexts.Roles.Read;

public sealed class RoleReadService(
    IRoleRepository roleRepo,
    IPermissionGrantRepository permissionGrantRepo) : IRoleReadService, IPersistenceService
{
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await roleRepo.GetAsync(r => r.Id == id, ct);
    }

    public async Task<Role?> GetByCodeAsync(RoleCode code, CancellationToken ct = default)
    {
        return await roleRepo.GetAsync(r => r.Code.Equals(code), ct);
    }

    public async Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct = default)
    {
        return await roleRepo.ListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetPermissionKeysAsync(Guid roleId, CancellationToken ct = default)
    {
        return await permissionGrantRepo.GetKeysByProviderAsync(PermissionProviderName.Role, roleId.ToString(), ct);
    }
}
