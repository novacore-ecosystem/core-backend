namespace NovaCore.Auth.Application.Security;

/// <summary>
/// Resolves a JWT's permission claims from the account's roles at token-issuance time. Stands in
/// for the full Role -&gt; RolePermission -&gt; PermissionDefinition catalog in Auth.Domain, which
/// exists but is not yet seeded/assignable - once that catalog is live this should be replaced by
/// reading Account.Permissions (the denormalized snapshot) instead of this static map.
/// </summary>
public static class RolePermissionMap
{
    private static readonly IReadOnlySet<string> RootPermissions = new HashSet<string>
    {
        Permissions.Root,
    };

    private static readonly IReadOnlySet<string> AdminPermissions = new HashSet<string>
    {
        Permissions.Product.Full,
        Permissions.Inventory.Full,
        Permissions.Warehouse.Full,
        Permissions.Order.Full,
        Permissions.Audit.Full,
        Permissions.Notification.Full,
        Permissions.Users.Full,
        Permissions.System.Full,
    };

    private static readonly IReadOnlySet<string> UserPermissions = new HashSet<string>();

    public static IReadOnlySet<string> Resolve(IEnumerable<string> roles)
    {
        var permissions = new HashSet<string>(StringComparer.Ordinal);

        foreach (var role in roles)
        {
            var rolePermissions = role switch
            {
                AppRoleConstant.Root => RootPermissions,
                AppRoleConstant.Admin => AdminPermissions,
                AppRoleConstant.User => UserPermissions,
                _ => (IReadOnlySet<string>)Array.Empty<string>().ToHashSet(),
            };

            permissions.UnionWith(rolePermissions);
        }

        return permissions;
    }
}
