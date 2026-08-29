using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// User service's own permissions (managing OTHER users' business-domain accounts - distinct from
/// the foundational `Permissions.User` platform-identity key in Permissions.Common.cs, which every
/// non-Root account carries for itself). Owned by the User team - see Permissions.Common.cs for
/// the file-splitting convention this follows.
/// </summary>
public static partial class Permissions
{
    [PermissionGroup("users")]
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
}
