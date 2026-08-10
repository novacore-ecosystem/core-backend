using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Web.Authorization;

public static class AuthorizationExtensions
{
    /// <summary>
    /// Registers everything RequirePermissions() needs at runtime. Centralized here so services
    /// never wire up authorization policies/handlers by hand - see PermissionEndpointExtensions.
    /// </summary>
    public static IServiceCollection AddBuildingBlockAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        return services;
    }
}
