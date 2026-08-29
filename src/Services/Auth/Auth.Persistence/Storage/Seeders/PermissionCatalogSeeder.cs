using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// Ensures one PermissionGroup per Permissions.cs module and one PermissionDefinition per
/// PermissionRegistry.Instance entry - the DB-backed catalog PermissionGrant rows point at.
/// Registry-driven, not a hardcoded catalog array: every permission key here is already a
/// [PermissionDefinition]-attributed C# const (see Permissions.cs) - this only backs the existing
/// platform-wide vocabulary with real rows, it does not define a second permission-definition
/// system (see docs/services/auth-service.md). Runs on every startup (not just an empty DB) and is
/// per-key idempotent, so a newly-added const gets its row created automatically on the next
/// deploy without a manual seed edit - existing rows (and any DB-owned metadata already on them,
/// e.g. translations/status) are never touched.
/// </summary>
public class PermissionCatalogSeeder(AuthDbContext context)
{
    public async Task SeedAsync()
    {
        var registryKeys = PermissionRegistry.Instance.GetAll().Select(d => d.Key).ToHashSet(StringComparer.Ordinal);

        var existingKeys = await context.PermissionDefinitions
            .Select(p => p.Key.Value)
            .ToHashSetAsync(StringComparer.Ordinal);

        var missingKeys = registryKeys.Except(existingKeys).ToList();
        if (missingKeys.Count == 0)
            return;

        var groupsByCode = await context.PermissionGroups
            .ToDictionaryAsync(g => g.Code.Value, StringComparer.Ordinal);

        foreach (var key in missingKeys)
        {
            var groupCode = GroupCodeFor(key);

            if (!groupsByCode.TryGetValue(groupCode, out var group))
            {
                group = PermissionGroup.Create(PermissionGroupCode.Create(groupCode));
                groupsByCode[groupCode] = group;
                await context.PermissionGroups.AddAsync(group);
            }

            var definition = PermissionDefinition.Create(
                PermissionKey.Create(key),
                group.Id,
                isSystemPermission: key is Permissions.Root or Permissions.User);

            await context.PermissionDefinitions.AddAsync(definition);
        }

        await context.SaveChangesAsync();
    }

    /// <summary>Mirrors the permission-key convention every const already follows
    /// ("module:action") - the module segment before the first ':' is the group code. Root/User
    /// are the one deliberate exception (grouped "platform", not "system", to keep the two
    /// mandatory platform-wide identity permissions visually distinct from System.*'s
    /// operational/messaging permissions despite sharing the "system:" key prefix) - preserved
    /// exactly from the prior hardcoded catalog to avoid a grouping regression.</summary>
    private static string GroupCodeFor(string permissionKey)
    {
        if (permissionKey is Permissions.Root or Permissions.User)
            return "platform";

        var separatorIndex = permissionKey.IndexOf(':');
        return separatorIndex < 0 ? permissionKey : permissionKey[..separatorIndex];
    }
}
