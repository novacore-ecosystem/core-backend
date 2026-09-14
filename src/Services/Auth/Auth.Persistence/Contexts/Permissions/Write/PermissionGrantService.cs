using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Persistence;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Write;

public sealed class PermissionGrantService(
    IPermissionGrantRepository permissionGrantRepo,
    IPermissionDefinitionRepository permissionDefinitionRepo,
    PermissionRegistry permissionRegistry,
    IUnitOfWork unitOfWork) : IPermissionGrantService, IPersistenceService
{
    public async Task GrantAsync(
        string permissionKey,
        PermissionProviderName providerName,
        string providerKey,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var definition = await permissionDefinitionRepo.GetAsync(p => p.Key.Value, permissionKey, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Permission \"{permissionKey}\" does not exist.");

        EnsureProviderAllowed(permissionKey, providerName);

        var alreadyGranted = await permissionGrantRepo.ExistsForProviderAsync(
            tenantId, definition.Id, providerName, providerKey, ct);
        if (alreadyGranted)
            return;

        await permissionGrantRepo.AddAsync(PermissionGrant.Create(definition.Id, providerName, providerKey), ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RevokeAsync(
        string permissionKey,
        PermissionProviderName providerName,
        string providerKey,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var grant = await permissionGrantRepo.GetForProviderAsync(tenantId, permissionKey, providerName, providerKey, ct);
        if (grant is null)
            return;

        await permissionGrantRepo.RemoveRangeAsync([grant], ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<PermissionGrantReplaceResult> ReplaceForProviderAsync(
        PermissionProviderName providerName,
        string providerKey,
        IReadOnlyCollection<string> permissionKeys,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var requestedKeys = permissionKeys.ToHashSet(StringComparer.Ordinal);

        var requestedDefinitions = await permissionDefinitionRepo.GetManyAsync(p => p.Key.Value, requestedKeys, ct);
        var definitionsByKey = requestedDefinitions.ToDictionary(p => p.Key.Value, StringComparer.Ordinal);

        // Known-but-disallowed is a hard rejection (the security boundary from
        // docs/services/auth-service.md's PermissionDefinitionAttribute design); unknown is a
        // silent skip, matching this endpoint's prior documented behavior.
        foreach (var key in requestedKeys)
        {
            if (definitionsByKey.ContainsKey(key))
                EnsureProviderAllowed(key, providerName);
        }

        var currentGrants = await permissionGrantRepo.ListForProviderAsync(tenantId, providerName, providerKey, ct);

        var currentKeys = currentGrants
            .Select(g => g.PermissionDefinition.Key.Value)
            .ToHashSet(StringComparer.Ordinal);

        var keysToRemove = currentKeys.Except(requestedKeys).ToList();
        var keysToAdd = requestedKeys.Except(currentKeys).Where(definitionsByKey.ContainsKey).ToList();

        var grantsToRemove = currentGrants.Where(g => keysToRemove.Contains(g.PermissionDefinition.Key.Value));
        await permissionGrantRepo.RemoveRangeAsync(grantsToRemove, ct);

        foreach (var key in keysToAdd)
            await permissionGrantRepo.AddAsync(
                PermissionGrant.Create(definitionsByKey[key].Id, providerName, providerKey),
                ct);

        var hasChanges = keysToRemove.Count > 0 || keysToAdd.Count > 0;

        var resultingKeys = currentKeys;
        resultingKeys.ExceptWith(keysToRemove);
        resultingKeys.UnionWith(keysToAdd);

        await unitOfWork.SaveChangesAsync(ct);

        return new PermissionGrantReplaceResult(hasChanges, resultingKeys);
    }

    public async Task<IReadOnlySet<string>> GetGrantedKeysAsync(
        PermissionProviderName providerName,
        string providerKey,
        Guid tenantId,
        CancellationToken ct = default)
    {
        return await permissionGrantRepo.GetGrantedKeysAsync(tenantId, providerName, providerKey, ct);
    }

    private void EnsureProviderAllowed(string permissionKey, PermissionProviderName providerName)
    {
        if (!permissionRegistry.IsProviderAllowed(permissionKey, providerName))
            throw ExceptionFactory.InvalidRange(
                $"Permission \"{permissionKey}\" cannot be granted to provider \"{providerName.ToName()}\".");
    }
}
