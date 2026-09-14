using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.SharedKernel.Authorization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// Ensures the seeded Root account directly holds every permission definition it is allowed to hold
/// as a User-provider grant, independent of the bypass it already gets through its Root role
/// membership - keeps the account's own grant inventory complete as new permissions are added.
/// </summary>
/// <remarks>
/// Requires PermissionCatalogSeeder and AccountSeeder to have run first. Skips
/// Permissions.Root itself - its [PermissionDefinition] restricts it to Role-provider grants only
/// (see Permissions.Common.cs), so PermissionRegistry.IsProviderAllowed is the source of truth here
/// rather than a hardcoded exclusion.
/// </remarks>
public class RootPermissionGrantSeeder(AuthDbContext context, IOptions<RootSetting> rootSetting)
{
    public async Task SeedAsync()
    {
        var rootAccountId = rootSetting.Value.RootId.ToString();

        var definitions = await context.PermissionDefinitions
            .Select(p => new { p.Id, Key = p.Key.Value })
            .ToListAsync();

        var grantedDefinitionIds = await context.PermissionGrants
            .Where(g => g.ProviderName == PermissionProviderName.User && g.ProviderKey == rootAccountId)
            .Select(g => g.PermissionDefinitionId)
            .ToHashSetAsync();

        var missingGrants = definitions
            .Where(d => !grantedDefinitionIds.Contains(d.Id))
            .Where(d => PermissionRegistry.Instance.IsProviderAllowed(d.Key, PermissionProviderName.User))
            .Select(d => PermissionGrant.Create(d.Id, PermissionProviderName.User, rootAccountId))
            .ToList();

        if (missingGrants.Count == 0)
            return;

        await context.PermissionGrants.AddRangeAsync(missingGrants);
        await context.SaveChangesAsync();
    }
}
