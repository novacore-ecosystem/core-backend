using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Web.Cors;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, string policyName)
    {
        string[] allowOrigins = ["http://localhost:3000", "http://localhost:5000"];
        services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
                policy.WithMethods(allowOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        return services;
    }

    public static WebApplication UseCorsPolicy(this WebApplication app, string policyName)
    {
        app.UseCors(policyName);
        return app;
    }
}
