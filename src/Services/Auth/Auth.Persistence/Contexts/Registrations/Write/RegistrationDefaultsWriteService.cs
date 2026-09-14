using NovaCore.Auth.Application.Abstractions.Persistence.Registrations;
using NovaCore.Auth.Domain.Entities.Registrations;
using NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;
using NovaCore.Auth.Persistence.Contexts.Registrations.Repositories;
using NovaCore.Auth.Persistence.Contexts.Roles.Repositories;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Persistence;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Persistence.Contexts.Registrations.Write;

public sealed class RegistrationDefaultsWriteService(
    IRegistrationDefaultRoleRepository registrationDefaultRoleRepo,
    IRegistrationDefaultPermissionRepository registrationDefaultPermissionRepo,
    IRoleRepository roleRepo,
    IPermissionDefinitionRepository permissionDefinitionRepo,
    PermissionRegistry permissionRegistry,
    IUnitOfWork unitOfWork) : IRegistrationDefaultsWriteService, IPersistenceService
{
    public async Task ReplaceDefaultRolesAsync(
        Guid tenantId, Guid appId, IReadOnlyCollection<Guid> roleIds, CancellationToken ct = default)
    {
        var requestedIds = roleIds.ToHashSet();
        var validRequestedIds = await roleRepo.GetExistingValuesAsync(r => r.Id, requestedIds, ct);

        var currentGrants = await registrationDefaultRoleRepo.ListForAppAsync(tenantId, appId, ct);
        var currentRoleIds = currentGrants.Select(x => x.RoleId).ToHashSet();

        var idsToAdd = validRequestedIds.Except(currentRoleIds).ToList();
        var grantsToRemove = currentGrants.Where(x => !validRequestedIds.Contains(x.RoleId));

        await registrationDefaultRoleRepo.RemoveRangeAsync(grantsToRemove, ct);

        foreach (var roleId in idsToAdd)
            await registrationDefaultRoleRepo.AddAsync(RegistrationDefaultRole.Create(appId, roleId), ct);

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ReplaceDefaultPermissionsAsync(
        Guid tenantId, Guid appId, IReadOnlyCollection<string> permissionKeys, CancellationToken ct = default)
    {
        var requestedKeys = permissionKeys.ToHashSet(StringComparer.Ordinal);

        var requestedDefinitions = await permissionDefinitionRepo.GetManyAsync(p => p.Key.Value, requestedKeys, ct);
        var definitionsByKey = requestedDefinitions.ToDictionary(p => p.Key.Value, StringComparer.Ordinal);

        // Known-but-disallowed is a hard rejection - reject a bad default when it's configured,
        // not the first time a user registers; unknown is a silent skip.
        foreach (var key in requestedKeys)
        {
            if (definitionsByKey.ContainsKey(key) && !permissionRegistry.IsProviderAllowed(key, PermissionProviderName.User))
                throw ExceptionFactory.InvalidRange(
                    $"Permission \"{key}\" cannot be granted to provider \"{PermissionProviderName.User.ToName()}\".");
        }

        var currentGrants = await registrationDefaultPermissionRepo.ListForAppAsync(tenantId, appId, ct);
        var currentKeys = currentGrants
            .Select(x => x.PermissionDefinition.Key.Value)
            .ToHashSet(StringComparer.Ordinal);

        var keysToAdd = requestedKeys.Except(currentKeys).Where(definitionsByKey.ContainsKey).ToList();
        var grantsToRemove = currentGrants.Where(x => !requestedKeys.Contains(x.PermissionDefinition.Key.Value));

        await registrationDefaultPermissionRepo.RemoveRangeAsync(grantsToRemove, ct);

        foreach (var key in keysToAdd)
            await registrationDefaultPermissionRepo.AddAsync(
                RegistrationDefaultPermission.Create(appId, definitionsByKey[key].Id), ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
