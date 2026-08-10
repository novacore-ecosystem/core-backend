using NovaCore.BuildingBlock.Web.ExceptionHandling;

using NovaCore.BuildingBlock.Application.DeadLetters;
using NovaCore.BuildingBlock.Grpc.Server;
using NovaCore.BuildingBlock.Infrastructure.Security.Jwt;
using NovaCore.BuildingBlock.Web;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.Web.Carter;
using NovaCore.BuildingBlock.Web.Cors;
using NovaCore.BuildingBlock.Web.CurrentUser;
using NovaCore.BuildingBlock.Web.HealthChecks;
using NovaCore.BuildingBlock.Web.Swagger;

namespace NovaCore.Auth.API;

public static class DependencyInjection
{
    internal static readonly BuildingBlockWebOptions WebOptions = new()
    {
        ServiceTitle = "NovaCore Auth Service",
        ServiceDescription = "Authentication and Authorization Service API",
        SwaggerRoutePrefix = "/api/auth",
        ContactUrl = "http://localhost:5100",
        SwaggerUiTitle = "Auth Service",
        IncludeJwtBearerSwaggerAuth = true,
        CorsPolicyName = "AllowAll",
        CorsAppliedPolicyName = "AllowGateway"
    };

    internal static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
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
            .AddGrpcServer()
            .AddBuildingBlockAuthorization();

        return services;
    }
}
