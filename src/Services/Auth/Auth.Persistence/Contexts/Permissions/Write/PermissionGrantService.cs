using Microsoft.EntityFrameworkCore;

using NovaCore.Auth.Application.Abstractions.Persistence.Permissions;
using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Contexts.Permissions.Repositories;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.Persistence;
using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.Auth.Persistence.Contexts.Permissions.Write;

/// <summary>The reference Persistence Service for the centralized grant model - a cross-aggregate
/// concern (PermissionDefinition/PermissionGrant, keyed by an arbitrary provider) exactly like
/// EffectivePermissionReadService, so it injects AuthDbContext directly for PermissionGrant queries
/// rather than a single-entity Repository abstraction. IPersistenceService makes it
/// auto-registered.</summary>
public sealed class PermissionGrantService(
    AuthDbContext dbContext,
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

        var alreadyGranted = await dbContext.PermissionGrants
            .IgnoreQueryFilters()
            .AnyAsync(
                g => g.TenantId == tenantId
                    && g.PermissionDefinitionId == definition.Id
                    && g.ProviderName == providerName
                    && g.ProviderKey == providerKey,
                ct);
        if (alreadyGranted)
            return;

        await dbContext.PermissionGrants.AddAsync(PermissionGrant.Create(definition.Id, providerName, providerKey), ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RevokeAsync(
        string permissionKey,
        PermissionProviderName providerName,
        string providerKey,
        Guid tenantId,
        CancellationToken ct = default)
    {
        var grant = await dbContext.PermissionGrants
            .IgnoreQueryFilters()
            .Include(g => g.PermissionDefinition)
            .FirstOrDefaultAsync(
                g => g.TenantId == tenantId
                    && g.PermissionDefinition.Key.Value == permissionKey
                    && g.ProviderName == providerName
                    && g.ProviderKey == providerKey,
                ct);
        if (grant is null)
            return;

        dbContext.PermissionGrants.Remove(grant);
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

        var currentGrants = await dbContext.PermissionGrants
            .IgnoreQueryFilters()
            .Include(g => g.PermissionDefinition)
            .Where(g => g.TenantId == tenantId && g.ProviderName == providerName && g.ProviderKey == providerKey)
            .ToListAsync(ct);

        var currentKeys = currentGrants
            .Select(g => g.PermissionDefinition.Key.Value)
            .ToHashSet(StringComparer.Ordinal);

        var keysToRemove = currentKeys.Except(requestedKeys).ToList();
        var keysToAdd = requestedKeys.Except(currentKeys).Where(definitionsByKey.ContainsKey).ToList();

        foreach (var key in keysToRemove)
        {
            var grant = currentGrants.First(g => g.PermissionDefinition.Key.Value == key);
            dbContext.PermissionGrants.Remove(grant);
        }

        foreach (var key in keysToAdd)
            await dbContext.PermissionGrants.AddAsync(
                PermissionGrant.Create(definitionsByKey[key].Id, providerName, providerKey),
                ct);

        var hasChanges = keysToRemove.Count > 0 || keysToAdd.Count > 0;

        var resultingKeys = currentKeys;
        resultingKeys.ExceptWith(keysToRemove);
        resultingKeys.UnionWith(keysToAdd);

        await unitOfWork.SaveChangesAsync(ct);

        return new PermissionGrantReplaceResult(hasChanges, resultingKeys);
    }

    private void EnsureProviderAllowed(string permissionKey, PermissionProviderName providerName)
    {
        if (!permissionRegistry.IsProviderAllowed(permissionKey, providerName))
            throw ExceptionFactory.InvalidRange(
                $"Permission \"{permissionKey}\" cannot be granted to provider \"{providerName.ToName()}\".");
    }
}
