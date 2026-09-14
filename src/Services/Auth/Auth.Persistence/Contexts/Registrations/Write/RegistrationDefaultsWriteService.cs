using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Registrations;
using NovaCore.Auth.Domain.Entities.Registrations;
using NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Persistence;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Persistence.Contexts.Registrations.Write;

public sealed class RegistrationDefaultsWriteService(
    AuthDbContext dbContext,
    IPermissionDefinitionRepository permissionDefinitionRepo,
    PermissionRegistry permissionRegistry,
    IUnitOfWork unitOfWork) : IRegistrationDefaultsWriteService, IPersistenceService
{
    public async Task ReplaceDefaultRolesAsync(
        Guid tenantId, Guid appId, IReadOnlyCollection<Guid> roleIds, CancellationToken ct = default)
    {
        var requestedIds = roleIds.ToHashSet();

        var existingRoleIds = await dbContext.Roles
            .Where(r => requestedIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(ct);
        var validRequestedIds = existingRoleIds.ToHashSet();

        var currentGrants = await dbContext.RegistrationDefaultRoles
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId && x.AppId == appId)
            .ToListAsync(ct);
        var currentRoleIds = currentGrants.Select(x => x.RoleId).ToHashSet();

        var idsToRemove = currentRoleIds.Except(validRequestedIds).ToList();
        var idsToAdd = validRequestedIds.Except(currentRoleIds).ToList();

        foreach (var roleId in idsToRemove)
            dbContext.RegistrationDefaultRoles.Remove(currentGrants.First(x => x.RoleId == roleId));

        foreach (var roleId in idsToAdd)
            await dbContext.RegistrationDefaultRoles.AddAsync(RegistrationDefaultRole.Create(appId, roleId), ct);

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

        var currentGrants = await dbContext.RegistrationDefaultPermissions
            .IgnoreQueryFilters()
            .Include(x => x.PermissionDefinition)
            .Where(x => x.TenantId == tenantId && x.AppId == appId)
            .ToListAsync(ct);
        var currentKeys = currentGrants
            .Select(x => x.PermissionDefinition.Key.Value)
            .ToHashSet(StringComparer.Ordinal);

        var keysToRemove = currentKeys.Except(requestedKeys).ToList();
        var keysToAdd = requestedKeys.Except(currentKeys).Where(definitionsByKey.ContainsKey).ToList();

        foreach (var key in keysToRemove)
            dbContext.RegistrationDefaultPermissions.Remove(
                currentGrants.First(x => x.PermissionDefinition.Key.Value == key));

        foreach (var key in keysToAdd)
            await dbContext.RegistrationDefaultPermissions.AddAsync(
                RegistrationDefaultPermission.Create(appId, definitionsByKey[key].Id), ct);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
