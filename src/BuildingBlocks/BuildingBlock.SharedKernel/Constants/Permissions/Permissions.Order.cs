using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Order service's own permissions. Owned by the Order team - see Permissions.Common.cs for the
/// file-splitting convention this follows.
/// </summary>
public static partial class Permissions
{
    [PermissionGroup("order")]
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
}
