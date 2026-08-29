using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Auth service's own business-domain permissions (Root Tenant Management). Owned by the Auth
/// team - see Permissions.Common.cs for the file-splitting convention this follows.
/// </summary>
public static partial class Permissions
{
    /// <summary>Root Tenant Management (see docs/services/auth-service.md) - distinct from the
    /// foundational `User` key in Permissions.Common.cs, which every non-Root account carries for
    /// its own tenant.</summary>
    [PermissionGroup("tenant")]
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
}
