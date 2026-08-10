using Microsoft.AspNetCore.Builder;

namespace NovaCore.BuildingBlock.Web.Authorization;

public static class PermissionEndpointExtensions
{
    /// <summary>
    /// Declares the permissions an endpoint accepts, OR-matched: the caller succeeds if they own
    /// any one of the listed permissions (exactly, via Permissions.Root, or via that permission's
    /// module aggregate "{module}:full" - see PermissionAuthorization.HasAnyPermission). This is
    /// the standard way to declare endpoint authorization across the project.
    /// </summary>
    public static TBuilder RequirePermissions<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        if (permissions.Length == 0)
            throw new ArgumentException("At least one permission must be specified.", nameof(permissions));

        return builder.RequireAuthorization(policy => policy
            .RequireAuthenticatedUser()
            .RequireAssertion(context => context.User.HasAnyPermission(permissions)));
    }
}
