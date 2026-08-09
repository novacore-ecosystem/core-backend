using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Roles;
using NovaCore.Auth.Application.Features.Roles.DTOs;
using NovaCore.Auth.Domain.Entities.Roles;
using NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;
using NovaCore.Auth.Persistence.Contexts.Roles.Repositories;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Persistence;

namespace NovaCore.Auth.Persistence.Contexts.Roles.Write;

public sealed class RoleWriteService(
    IRoleRepository repo,
    IPermissionDefinitionRepository permissionRepo,
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

    public async Task<RolePermissionUpdateResult> UpdatePermissionsAsync(
        Guid id,
        IReadOnlyCollection<string> permissionKeys,
        CancellationToken ct = default)
    {
        var requestedKeys = permissionKeys.ToHashSet(StringComparer.Ordinal);

        var requestedPermissions = await permissionRepo.GetManyAsync(p => p.Key.Value, requestedKeys, ct);
        var permissionsByKey = requestedPermissions.ToDictionary(p => p.Key.Value, StringComparer.Ordinal);

        var hasChanges = false;
        var resultingKeys = new HashSet<string>(StringComparer.Ordinal);

        await repo.UpdateAsync(
            r => r.Id == id,
            q => q.Include(r => r.Permissions).ThenInclude(rp => rp.PermissionDefinition),
            role =>
            {
                var currentKeys = role.Permissions
                    .Select(rp => rp.PermissionDefinition.Key.Value)
                    .ToHashSet(StringComparer.Ordinal);

                // Removals are resolved and applied before any AssignPermission call below -
                // newly-added RolePermission rows carry no PermissionDefinition navigation (see
                // RolePermission.Create), so touching rp.PermissionDefinition after an Add would
                // risk a null reference if this loop ran over the mutated collection instead.
                var keysToRemove = currentKeys.Except(requestedKeys).ToList();
                foreach (var key in keysToRemove)
                {
                    var permissionDefinitionId = role.Permissions
                        .First(rp => rp.PermissionDefinition.Key.Value == key)
                        .PermissionDefinitionId;
                    role.RemovePermission(permissionDefinitionId);
                }

                var addedKeys = new List<string>();
                foreach (var key in requestedKeys.Except(currentKeys))
                {
                    if (!permissionsByKey.TryGetValue(key, out var permission))
                        continue;

                    role.AssignPermission(permission);
                    addedKeys.Add(key);
                }

                hasChanges = keysToRemove.Count > 0 || addedKeys.Count > 0;
                resultingKeys = currentKeys;
                resultingKeys.ExceptWith(keysToRemove);
                resultingKeys.UnionWith(addedKeys);
            },
            ct);

        await unitOfWork.SaveChangesAsync(ct);

        return new RolePermissionUpdateResult(hasChanges, [.. resultingKeys]);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await repo.DeleteAsync(r => r.Id == id, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
