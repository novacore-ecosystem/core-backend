using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// Ensures one PermissionGroup and one PermissionDefinition (with its initial translations) exist
/// per <see cref="PermissionSeedCatalog"/> entry - the DB-backed catalog PermissionGrant rows point
/// at. Group identity comes from each permission's [PermissionGroup]-attributed enclosing class (see
/// PermissionRegistry); a permission the registry reports as ungrouped (Root/User) falls back to the
/// fixed "platform" DB group. This only backs the existing platform-wide vocabulary with real rows,
/// it does not define a second permission-definition system (see docs/services/auth-service.md).
/// Runs on every startup (not just an empty DB) and is per-key idempotent: an already-existing
/// definition is never touched (translations included) - only a key missing from the catalog gets a
/// definition and its supplied translations created.
/// </summary>
public class PermissionCatalogSeeder(AuthDbContext context)
{
    private const string UngroupedFallbackGroupCode = "platform";

    public async Task SeedAsync()
    {
        var existingKeys = await context.PermissionDefinitions
            .Select(p => p.Key.Value)
            .ToHashSetAsync(StringComparer.Ordinal);

        var missingSeeds = PermissionSeedCatalog.All.Where(s => !existingKeys.Contains(s.Key)).ToList();
        if (missingSeeds.Count == 0)
            return;

        var groupsByCode = await context.PermissionGroups
            .ToDictionaryAsync(g => g.Code.Value, StringComparer.Ordinal);

        var definitions = missingSeeds
            .Select(seed => BuildDefinition(seed, ResolveGroup(seed.Key, groupsByCode)))
            .ToList();

        await context.PermissionDefinitions.AddRangeAsync(definitions);
        await context.SaveChangesAsync();
    }

    private PermissionGroup ResolveGroup(string key, Dictionary<string, PermissionGroup> groupsByCode)
    {
        var groupCode = PermissionRegistry.Instance.Get(key)?.GroupCode ?? UngroupedFallbackGroupCode;
        if (groupsByCode.TryGetValue(groupCode, out var group))
            return group;

        group = PermissionGroup.Create(PermissionGroupCode.Create(groupCode));
        groupsByCode[groupCode] = group;
        context.PermissionGroups.Add(group);

        return group;
    }

    private static PermissionDefinition BuildDefinition(PermissionSeed seed, PermissionGroup group)
    {
        var definition = PermissionDefinition.Create(
            PermissionKey.Create(seed.Key),
            group.Id,
            isSystemPermission: seed.Key is Permissions.Root or Permissions.User);

        definition.Translate(LanguageCode.Create(LanguageCodeConstant.English), seed.EnglishDisplayName);
        if (seed.VietnameseDisplayName is not null)
            definition.Translate(LanguageCode.Create(LanguageCodeConstant.Vietnamese), seed.VietnameseDisplayName);

        return definition;
    }
}
