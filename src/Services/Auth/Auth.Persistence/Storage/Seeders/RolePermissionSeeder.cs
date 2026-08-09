using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.SharedKernel.Constants;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// Grants the seeded system Roles (Root/Admin/User) their permissions as real RolePermission
/// rows, replacing what RolePermissionMap used to hard-code in C# (see
/// docs/services/auth-service.md, Phase 3). Mirrors RolePermissionMap's exact former mapping for
/// Root/Admin so removing it is not a behavior regression; User additionally gains the new
/// mandatory Permissions.User grant (RolePermissionMap left User with zero permissions).
/// Runs via TenantAssignmentInterceptor like any other write - seeding has no RequestContext, so
/// every grant lands on TenantId == Guid.Empty, the same global/Root scope Account's own seeded
/// Root row uses. Requires RoleSeeder and PermissionCatalogSeeder to have run first.
/// </summary>
public class RolePermissionSeeder(AuthDbContext context)
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
        if (await context.RolePermissions.AnyAsync())
            return;

        var roles = await context.Roles
            .Include(r => r.Permissions)
            .Where(r => GrantsByRoleName.Keys.Contains(r.Name!))
            .ToDictionaryAsync(r => r.Name!, r => r);

        var permissionsByKey = await context.PermissionDefinitions
            .ToDictionaryAsync(p => p.Key.Value, p => p);

        foreach (var (roleName, permissionKeys) in GrantsByRoleName)
        {
            if (!roles.TryGetValue(roleName, out var role))
                continue;

            foreach (var permissionKey in permissionKeys)
            {
                if (permissionsByKey.TryGetValue(permissionKey, out var permission))
                    role.AssignPermission(permission);
            }
        }

        await context.SaveChangesAsync();
    }
}
