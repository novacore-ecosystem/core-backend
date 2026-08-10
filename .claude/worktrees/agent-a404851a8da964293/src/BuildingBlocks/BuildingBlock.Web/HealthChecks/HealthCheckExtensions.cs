using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Web.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }
}
