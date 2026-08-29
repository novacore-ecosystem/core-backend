using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Every permission key the platform recognizes, grouped by business capability. Permission keys
/// are code-first (declared here, discovered into PermissionRegistry, seeded into Auth's
/// PermissionDefinition catalog, referenced by RequirePermissions() on endpoints) - never
/// free-form user input - so PermissionKey validates against PermissionRegistry.Instance instead
/// of a runtime format check.
///
/// Every const carries a [PermissionDefinition(Providers = ...)] declaring which authorization
/// provider categories may hold a grant for it (see PermissionRegistry/PermissionGrant). All
/// default to Role-only - Role is the only provider this milestone wires a grant path for; a
/// future direct User/Client/Guest grant just needs its Providers flags widened here, not a new
/// table.
///
/// Root bypasses every check. Each module's "Full" key is an aggregate that implicitly grants
/// every other permission in that module - this is resolved centrally by
/// PermissionAuthorization.HasAnyPermission (BB.Web), endpoints never need to declare it explicitly.
/// </summary>
public static class Permissions
{
    [PermissionDefinition(Providers = PermissionProviderName.Role)]
    public const string Root = "system:root";

    /// <summary>The foundational Tenant/Client user capability - every authenticated non-Root
    /// account carries this. Distinct from Users.* below (managing OTHER users' accounts).</summary>
    [PermissionDefinition(Providers = PermissionProviderName.Role)]
    public const string User = "system:user";

    public static class Role
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "role:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Manage = "role:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "role:full";
    }

    public static class Permission
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "permission:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Manage = "permission:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "permission:full";
    }

    public static class Product
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Manage = "product:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Reindex = "product:reindex";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "product:full";
    }

    public static class Inventory
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "inventory:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string StockMove = "inventory:stock-move";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Adjust = "inventory:adjust";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Receive = "inventory:receive";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Transfer = "inventory:transfer";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string CycleCount = "inventory:cycle-count";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "inventory:full";
    }

    public static class Warehouse
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "warehouse:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Manage = "warehouse:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "warehouse:full";
    }

    public static class Order
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "order:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Manage = "order:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Fulfill = "order:fulfill";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Delete = "order:delete";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string CreateOnBehalf = "order:create-on-behalf";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "order:full";
    }

    public static class Audit
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "audit:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "audit:full";
    }

    public static class Notification
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "notification:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Manage = "notification:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string ChannelToggle = "notification:channel-toggle";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string ChannelConfigure = "notification:channel-configure";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string CampaignManage = "notification:campaign-manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Send = "notification:send";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "notification:full";
    }

    public static class Users
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "users:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Manage = "users:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Reindex = "users:reindex";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "users:full";
    }

    /// <summary>Root Tenant Management (see docs/services/auth-service.md) - distinct from the
    /// foundational `User` key above, which every non-Root account carries for its own tenant.</summary>
    public static class Tenant
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "tenant:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Manage = "tenant:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string RotateClient = "tenant:rotate-client";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "tenant:full";
    }

    /// <summary>Platform-operational capabilities (e.g. dead-letter queue management) not owned by any single business module.</summary>
    public static class System
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string MessagingView = "system:messaging-view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string MessagingRequeue = "system:messaging-requeue";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "system:full";
    }
}
