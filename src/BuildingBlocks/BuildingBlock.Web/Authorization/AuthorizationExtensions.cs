using NovaCore.BuildingBlock.Application.Abstractions.Authorization;
using NovaCore.BuildingBlock.Application.Authorization;

using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Web.Authorization;

public static class AuthorizationExtensions
{
    /// <summary>
    /// Registers everything RequirePermissions()/IAuthorizationGuard need at runtime. Centralized
    /// here so services never wire up authorization policies/handlers by hand - see
    /// PermissionEndpointExtensions for endpoint-level checks and IAuthorizationGuard for
    /// Application-layer ones.
    /// </summary>
    public static IServiceCollection AddBuildingBlockAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddScoped<IAuthorizationGuard, AuthorizationGuard>();
        return services;
    }
}
