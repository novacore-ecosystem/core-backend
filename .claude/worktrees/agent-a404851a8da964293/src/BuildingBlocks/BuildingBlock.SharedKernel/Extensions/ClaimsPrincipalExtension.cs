using System.Security.Claims;

using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.BuildingBlock.SharedKernel.Extensions;

/// <summary>
/// Reads raw claim values off a ClaimsPrincipal. No authorization decisions are made here - see
/// BB.Web's Authorization namespace for permission evaluation built on top of these reads.
/// </summary>
public static class ClaimsPrincipalExtension
{
    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal principal)
        => principal.FindAll(AppClaimTypes.Permission).Select(c => c.Value);
}
