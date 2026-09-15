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
        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User | PermissionProviderName.Tenant)]
        public const string View = "tenant:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User | PermissionProviderName.Tenant)]
        public const string Manage = "tenant:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User | PermissionProviderName.Tenant)]
        public const string RotateClient = "tenant:rotate-client";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User | PermissionProviderName.Tenant)]
        public const string Full = "tenant:full";
    }

    /// <summary>App Management - the client-application boundary (see App.cs's class doc
    /// comment) a user is assigned to, distinct from Tenant.</summary>
    [PermissionGroup("app")]
    public static class App
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string View = "app:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string Manage = "app:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string AssignUsers = "app:assign-users";
    }

    /// <summary>Default-Role/Default-Permission configuration granted to every Account that
    /// self-registers into a given (Tenant, App) pair - see RegistrationDefaultRole/
    /// RegistrationDefaultPermission and RegisterHandler.</summary>
    [PermissionGroup("registration-defaults")]
    public static class RegistrationDefaults
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string View = "registration-defaults:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string Manage = "registration-defaults:manage";
    }
}
