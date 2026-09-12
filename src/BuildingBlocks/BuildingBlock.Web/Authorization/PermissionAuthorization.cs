using System.Security.Claims;

using NovaCore.BuildingBlock.Application.Abstractions.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.BuildingBlock.Web.Authorization;

/// <summary>
/// Endpoint-level permission evaluation - the OR-matching that RequirePermissions() delegates to.
/// Leaf-key matching (Root bypass, "{module}:full" aggregation) itself lives in
/// <see cref="PermissionExpression.IsGranted"/>, shared with the Application-layer
/// <c>IAuthorizationGuard</c> so the rule is defined once for both endpoint and use-case checks.
/// </summary>
public static class PermissionAuthorization
{
    /// <summary>
    /// The caller succeeds if they satisfy any one of the given permissions.
    /// </summary>
    public static bool HasAnyPermission(this ClaimsPrincipal principal, params string[] permissions)
    {
        var owned = principal.GetPermissions().ToHashSet(StringComparer.Ordinal);
        return permissions.Any(required => PermissionExpression.IsGranted(owned, required));
    }
}
