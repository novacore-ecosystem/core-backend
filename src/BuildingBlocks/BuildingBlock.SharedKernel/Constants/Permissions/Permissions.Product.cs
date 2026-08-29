using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Product service's own permissions. Owned by the Product team - see Permissions.Common.cs for
/// the file-splitting convention this follows.
/// </summary>
public static partial class Permissions
{
    [PermissionGroup("product")]
    public static class Product
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Manage = "product:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Reindex = "product:reindex";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "product:full";
    }
}
