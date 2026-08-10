using System.Collections.Frozen;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Every permission key the platform recognizes, grouped by business capability. Permission keys
/// are code-first (declared here, seeded into Auth's PermissionDefinition catalog, referenced by
/// RequirePermissions() on endpoints) - never free-form user input - so PermissionKey validates
/// against SupportedValues instead of a runtime format check.
///
/// Root bypasses every check. Each module's "Full" key is an aggregate that implicitly grants
/// every other permission in that module - this is resolved centrally by
/// PermissionAuthorization.HasAnyPermission (BB.Web), endpoints never need to declare it explicitly.
/// </summary>
public static class Permissions
{
    public const string Root = "system:root";

    public static class Product
    {
        public const string Manage = "product:manage";
        public const string Reindex = "product:reindex";
        public const string Full = "product:full";
    }

    public static class Inventory
    {
        public const string View = "inventory:view";
        public const string StockMove = "inventory:stock-move";
        public const string Adjust = "inventory:adjust";
        public const string Receive = "inventory:receive";
        public const string Transfer = "inventory:transfer";
        public const string CycleCount = "inventory:cycle-count";
        public const string Full = "inventory:full";
    }

    public static class Warehouse
    {
        public const string View = "warehouse:view";
        public const string Manage = "warehouse:manage";
        public const string Full = "warehouse:full";
    }

    public static class Order
    {
        public const string View = "order:view";
        public const string Manage = "order:manage";
        public const string Fulfill = "order:fulfill";
        public const string Delete = "order:delete";
        public const string CreateOnBehalf = "order:create-on-behalf";
        public const string Full = "order:full";
    }

    public static class Audit
    {
        public const string View = "audit:view";
        public const string Full = "audit:full";
    }

    public static class Notification
    {
        public const string View = "notification:view";
        public const string Manage = "notification:manage";
        public const string ChannelToggle = "notification:channel-toggle";
        public const string ChannelConfigure = "notification:channel-configure";
        public const string CampaignManage = "notification:campaign-manage";
        public const string Send = "notification:send";
        public const string Full = "notification:full";
    }

    public static class Users
    {
        public const string View = "users:view";
        public const string Manage = "users:manage";
        public const string Reindex = "users:reindex";
        public const string Full = "users:full";
    }

    /// <summary>Platform-operational capabilities (e.g. dead-letter queue management) not owned by any single business module.</summary>
    public static class System
    {
        public const string MessagingView = "system:messaging-view";
        public const string MessagingRequeue = "system:messaging-requeue";
        public const string Full = "system:full";
    }

    public static readonly FrozenSet<string> SupportedValues = new[]
    {
        Root,
        Product.Manage, Product.Reindex, Product.Full,
        Inventory.View, Inventory.StockMove, Inventory.Adjust, Inventory.Receive, Inventory.Transfer, Inventory.CycleCount, Inventory.Full,
        Warehouse.View, Warehouse.Manage, Warehouse.Full,
        Order.View, Order.Manage, Order.Fulfill, Order.Delete, Order.CreateOnBehalf, Order.Full,
        Audit.View, Audit.Full,
        Notification.View, Notification.Manage, Notification.ChannelToggle, Notification.ChannelConfigure, Notification.CampaignManage, Notification.Send, Notification.Full,
        Users.View, Users.Manage, Users.Reindex, Users.Full,
        System.MessagingView, System.MessagingRequeue, System.Full,
    }.ToFrozenSet();
}
