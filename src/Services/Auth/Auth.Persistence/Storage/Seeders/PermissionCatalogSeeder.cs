using NovaCore.Auth.Domain.Entities.Permissions;
using NovaCore.Auth.Domain.ValueObjects;
using NovaCore.Auth.Persistence.Engine;

using NovaCore.BuildingBlock.SharedKernel.Constants;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// Seeds one PermissionGroup per Permissions.cs module and one PermissionDefinition per
/// Permissions.SupportedValues entry - the DB-backed catalog RolePermission grants point at.
/// Mechanical: every key seeded here is already a declared C# constant (see Permissions.cs),
/// never an invented one - this only backs the existing platform-wide vocabulary with real rows,
/// it does not define a second permission-definition system (see docs/services/auth-service.md).
/// </summary>
public class PermissionCatalogSeeder(AuthDbContext context)
{
    private static readonly (string GroupCode, string Key)[] Catalog =
    [
        ("platform", Permissions.Root),
        ("platform", Permissions.User),

        ("role", Permissions.Role.View),
        ("role", Permissions.Role.Manage),
        ("role", Permissions.Role.Full),

        ("permission", Permissions.Permission.View),
        ("permission", Permissions.Permission.Manage),
        ("permission", Permissions.Permission.Full),

        ("product", Permissions.Product.Manage),
        ("product", Permissions.Product.Reindex),
        ("product", Permissions.Product.Full),

        ("inventory", Permissions.Inventory.View),
        ("inventory", Permissions.Inventory.StockMove),
        ("inventory", Permissions.Inventory.Adjust),
        ("inventory", Permissions.Inventory.Receive),
        ("inventory", Permissions.Inventory.Transfer),
        ("inventory", Permissions.Inventory.CycleCount),
        ("inventory", Permissions.Inventory.Full),

        ("warehouse", Permissions.Warehouse.View),
        ("warehouse", Permissions.Warehouse.Manage),
        ("warehouse", Permissions.Warehouse.Full),

        ("order", Permissions.Order.View),
        ("order", Permissions.Order.Manage),
        ("order", Permissions.Order.Fulfill),
        ("order", Permissions.Order.Delete),
        ("order", Permissions.Order.CreateOnBehalf),
        ("order", Permissions.Order.Full),

        ("audit", Permissions.Audit.View),
        ("audit", Permissions.Audit.Full),

        ("notification", Permissions.Notification.View),
        ("notification", Permissions.Notification.Manage),
        ("notification", Permissions.Notification.ChannelToggle),
        ("notification", Permissions.Notification.ChannelConfigure),
        ("notification", Permissions.Notification.CampaignManage),
        ("notification", Permissions.Notification.Send),
        ("notification", Permissions.Notification.Full),

        ("users", Permissions.Users.View),
        ("users", Permissions.Users.Manage),
        ("users", Permissions.Users.Reindex),
        ("users", Permissions.Users.Full),

        ("system", Permissions.System.MessagingView),
        ("system", Permissions.System.MessagingRequeue),
        ("system", Permissions.System.Full),
    ];

    public async Task SeedAsync()
    {
        if (await context.PermissionDefinitions.AnyAsync())
            return;

        var groups = Catalog
            .Select(x => x.GroupCode)
            .Distinct()
            .ToDictionary(code => code, code => PermissionGroup.Create(PermissionGroupCode.Create(code)));

        await context.PermissionGroups.AddRangeAsync(groups.Values);

        var definitions = Catalog.Select(x => PermissionDefinition.Create(
            PermissionKey.Create(x.Key),
            groups[x.GroupCode].Id,
            isSystemPermission: x.Key is Permissions.Root or Permissions.User));

        await context.PermissionDefinitions.AddRangeAsync(definitions);

        await context.SaveChangesAsync();
    }
}
