using NovaCore.BuildingBlock.SharedKernel.Authorization;

namespace NovaCore.BuildingBlock.SharedKernel.Constants;

/// <summary>
/// Every permission key the platform recognizes. Physically split across
/// Permissions.&lt;Owner&gt;.cs files (this one, Common, plus one per owning service) purely for Git
/// ownership - unrelated teams editing unrelated services never touch the same file - while
/// staying one unified `public static partial class Permissions` API for consumers (see
/// docs/conventions/permission-catalog-conventions.md for the full convention).
///
/// This file holds genuinely common/system-level permissions only: the two mandatory platform
/// identity permissions (Root/User - deliberately not nested under any [PermissionGroup], see
/// PermissionGroupAttribute's doc comment), Auth's own Role/Permission management permissions, and
/// System's platform-operational permissions not owned by any single business module. Do not add a
/// service-specific permission here - give that service its own Permissions.&lt;Service&gt;.cs file
/// instead, even for a single permission.
///
/// Every const carries a [PermissionDefinition(Providers = ...)] declaring which authorization
/// provider categories may hold a grant for it (see PermissionRegistry/PermissionGrant). Every
/// permission except Root allows both Role and direct User grants (the Auth service's
/// Account-authorization endpoints let an actor grant/revoke any of these directly on an
/// Account, in addition to via a Role) - Client/Guest/ServiceAccount grant paths remain
/// unwired, so those flags stay unused for now.
///
/// Root bypasses every check and can never itself be granted through the normal grant flow -
/// Providers stays Role-only, the same restriction the Application-layer grant validation
/// enforces (see IAccountAuthorizationGuard) - it is a provisioning/DB-seed-only concern. Each
/// module's "Full" key is an aggregate that implicitly grants every other permission in that
/// module - this is resolved centrally by PermissionAuthorization.HasAnyPermission (BB.Web),
/// endpoints never need to declare it explicitly.
/// </summary>
public static partial class Permissions
{
    [PermissionDefinition(Providers = PermissionProviderName.Role)]
    public const string Root = "system:root";

    /// <summary>The foundational Tenant/Client user capability - every authenticated non-Root
    /// account carries this. Distinct from Users.* (User service, managing OTHER users'
    /// accounts).</summary>
    [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
    public const string User = "system:user";

    [PermissionGroup("role")]
    public static class Role
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string View = "role:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string Manage = "role:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string Full = "role:full";
    }

    [PermissionGroup("permission")]
    public static class Permission
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string View = "permission:view";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string Manage = "permission:manage";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string Full = "permission:full";
    }

    /// <summary>Platform-operational capabilities (e.g. dead-letter queue management) not owned by any single business module.</summary>
    [PermissionGroup("system")]
    public static class System
    {
        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string MessagingView = "system:messaging-view";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string MessagingRequeue = "system:messaging-requeue";

        [PermissionDefinition(Providers = PermissionProviderName.Role | PermissionProviderName.User)]
        public const string Full = "system:full";
    }
}
