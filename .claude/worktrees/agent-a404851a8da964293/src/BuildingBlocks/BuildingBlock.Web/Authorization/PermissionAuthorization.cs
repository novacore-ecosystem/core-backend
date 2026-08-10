using System.Security.Claims;

using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.BuildingBlock.Web.Authorization;

/// <summary>
/// Permission evaluation - the authorization decision-making that RequirePermissions() delegates
/// to. This is the single place that logic lives; everything else just reads claims.
/// </summary>
public static class PermissionAuthorization
{
    /// <summary>
    /// Root bypasses everything, then each required permission is checked exact-match or via its
    /// module's aggregate "{module}:full" key.
    /// </summary>
    public static bool HasAnyPermission(this ClaimsPrincipal principal, params string[] permissions)
    {
        var owned = principal.GetPermissions().ToHashSet(StringComparer.Ordinal);

        if (owned.Contains(Permissions.Root))
            return true;

        foreach (var required in permissions)
        {
            if (owned.Contains(required))
                return true;

            var separatorIndex = required.IndexOf(':');
            if (separatorIndex > 0)
            {
                var aggregate = $"{required[..separatorIndex]}:full";
                if (owned.Contains(aggregate))
                    return true;
            }
        }

        return false;
    }
}
