using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.SharedKernel.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// Grants the seeded system Roles (Root/Admin/User) their permissions as PermissionGrant rows
/// (ProviderName = Role, ProviderKey = that Role's Id) - the centralized replacement for the
/// former Role-only RolePermission grant. Mirrors the prior RolePermissionMap/RolePermissionSeeder
/// mapping for Root/Admin exactly (no behavior regression); User additionally carries the mandatory
/// Permissions.User grant. Runs via TenantAssignmentInterceptor like any other write - seeding has
/// no RequestContext, so every grant lands on TenantId == Guid.Empty, the same global/Root scope
/// Account's own seeded Root row uses. Requires RoleSeeder and PermissionCatalogSeeder to have run
/// first.
/// </summary>
public class RoleGrantSeeder(AuthDbContext context)
{
    private static readonly IReadOnlyDictionary<string, string[]> GrantsByRoleName = new Dictionary<string, string[]>
    {
        [SeedData.Roles.Root] = [Permissions.Root],
        [SeedData.Roles.Admin] =
        [
            Permissions.Product.Full,
            Permissions.Inventory.Full,
            Permissions.Warehouse.Full,
            Permissions.Order.Full,
            Permissions.Audit.Full,
            Permissions.Notification.Full,
            Permissions.Users.Full,
            Permissions.System.Full,
        ],
        [SeedData.Roles.User] = [Permissions.User],
    };

    public async Task SeedAsync()
    {
        if (await context.PermissionGrants.AnyAsync())
            return;

        var roles = await context.Roles
            .Where(r => GrantsByRoleName.Keys.Contains(r.Name!))
            .ToDictionaryAsync(r => r.Name!);

        var definitionIdsByKey = await context.PermissionDefinitions
            .ToDictionaryAsync(p => p.Key.Value, p => p.Id, StringComparer.Ordinal);

        foreach (var (roleName, permissionKeys) in GrantsByRoleName)
        {
            if (!roles.TryGetValue(roleName, out var role))
                continue;

            foreach (var permissionKey in permissionKeys)
            {
                if (!definitionIdsByKey.TryGetValue(permissionKey, out var definitionId))
                    continue;

                await context.PermissionGrants.AddAsync(
                    PermissionGrant.Create(definitionId, PermissionProviderName.Role, role.Id.ToString()));
            }
        }

        await context.SaveChangesAsync();
    }
}
