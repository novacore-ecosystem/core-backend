using NovaCore.BuildingBlock.Application.DeadLetters;
using NovaCore.BuildingBlock.Infrastructure.Security.Jwt;
using NovaCore.BuildingBlock.Web;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.Web.Carter;
using NovaCore.BuildingBlock.Web.Cors;
using NovaCore.BuildingBlock.Web.CurrentUser;
using NovaCore.BuildingBlock.Web.HealthChecks;
using NovaCore.BuildingBlock.Web.Swagger;

using NovaCore.BuildingBlock.Web.ExceptionHandling;

namespace NovaCore.Payment.API;

public static class DependencyInjection
{
    internal static readonly BuildingBlockWebOptions WebOptions = new()
    {
        ServiceTitle = "NovaCore Payment Service",
        ServiceDescription = "Payment Gateway Service API",
        SwaggerRoutePrefix = "/api/payment",
        ContactUrl = "http://localhost:5109",
        SwaggerUiTitle = "Payment Service",
        IncludeJwtBearerSwaggerAuth = false,
        CorsPolicyName = "AllowAll"
    };

    internal static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddExceptionHandling()
            .AddCurrentUser()
            .AddJwtBearerAuthentication(configuration)
            .AddSwaggerDocumentation(WebOptions)
            .AddCorsPolicy(WebOptions.CorsPolicyName)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IDeadLetterRetryService).Assembly))
            .AddDeadLetterManagement()
            .AddCarterModules(typeof(DependencyInjection), typeof(IDeadLetterRetryService))
            .AddHealthCheckServices()
            .AddBuildingBlockAuthorization();

        return services;
    }
}
