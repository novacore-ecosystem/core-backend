using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Notification service's own permissions. Owned by the Notification team - see
/// Permissions.Common.cs for the file-splitting convention this follows.
/// </summary>
public static partial class Permissions
{
    [PermissionGroup("notification")]
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
}
