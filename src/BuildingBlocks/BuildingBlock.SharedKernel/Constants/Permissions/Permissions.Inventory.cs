using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Inventory service's own permissions (stock and warehouse management - two separate permission
/// groups, same owning service). Owned by the Inventory team - see Permissions.Common.cs for the
/// file-splitting convention this follows.
/// </summary>
public static partial class Permissions
{
    [PermissionGroup("inventory")]
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

    [PermissionGroup("warehouse")]
    public static class Warehouse
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "warehouse:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Manage = "warehouse:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "warehouse:full";
    }
}
