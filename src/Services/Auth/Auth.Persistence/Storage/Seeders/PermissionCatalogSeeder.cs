using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// Ensures one PermissionGroup per PermissionRegistry.Instance group and one PermissionDefinition
/// per registry entry - the DB-backed catalog PermissionGrant rows point at. Registry-driven, not a
/// hardcoded catalog array or a key-prefix heuristic: group identity comes directly from each
/// permission's [PermissionGroup]-attributed enclosing class (see PermissionRegistry), and a
/// permission the registry reports as ungrouped (Root/User - deliberately not nested under any
/// [PermissionGroup], see Permissions.Common.cs) falls back to the fixed "platform" DB group. This
/// only backs the existing platform-wide vocabulary with real rows, it does not define a second
/// permission-definition system (see docs/services/auth-service.md). Runs on every startup (not
/// just an empty DB) and is per-key idempotent, so a newly-added const in any
/// Permissions.&lt;Owner&gt;.cs file gets its row created automatically on the next deploy without
/// a manual seed edit - existing rows (and any DB-owned metadata already on them, e.g.
/// translations/status) are never touched.
/// </summary>
public class PermissionCatalogSeeder(AuthDbContext context)
{
    private const string UngroupedFallbackGroupCode = "platform";

    public async Task SeedAsync()
    {
        var registryDefinitions = PermissionRegistry.Instance.GetAll();

        var existingKeys = await context.PermissionDefinitions
            .Select(p => p.Key.Value)
            .ToHashSetAsync(StringComparer.Ordinal);

        var missingDefinitions = registryDefinitions.Where(d => !existingKeys.Contains(d.Key)).ToList();
        if (missingDefinitions.Count == 0)
            return;

        var groupsByCode = await context.PermissionGroups
            .ToDictionaryAsync(g => g.Code.Value, StringComparer.Ordinal);

        foreach (var definition in missingDefinitions)
        {
            var groupCode = definition.GroupCode ?? UngroupedFallbackGroupCode;

            if (!groupsByCode.TryGetValue(groupCode, out var group))
            {
                group = PermissionGroup.Create(PermissionGroupCode.Create(groupCode));
                groupsByCode[groupCode] = group;
                await context.PermissionGroups.AddAsync(group);
            }

            var permissionDefinition = PermissionDefinition.Create(
                PermissionKey.Create(definition.Key),
                group.Id,
                isSystemPermission: definition.Key is Permissions.Root or Permissions.User);

            await context.PermissionDefinitions.AddAsync(permissionDefinition);
        }

        await context.SaveChangesAsync();
    }
}
