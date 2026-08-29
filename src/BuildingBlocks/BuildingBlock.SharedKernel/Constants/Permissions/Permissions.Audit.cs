using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Audit service's own permissions. Owned by the Audit team - see Permissions.Common.cs for the
/// file-splitting convention this follows.
/// </summary>
public static partial class Permissions
{
    [PermissionGroup("audit")]
    public static class Audit
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string View = "audit:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role)]
        public const string Full = "audit:full";
    }
}
