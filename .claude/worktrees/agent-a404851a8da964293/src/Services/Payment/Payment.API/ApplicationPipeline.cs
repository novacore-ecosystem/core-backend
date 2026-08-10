using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Web.Cors;
using NovaCore.BuildingBlock.Web.Middleware;
using NovaCore.BuildingBlock.Web.Swagger;

using NovaCore.BuildingBlock.Web.ExceptionHandling;

using NovaCore.Payment.Persistence.Storage.Seeders;

namespace NovaCore.Payment.API;

public static class ApplicationPipeline
{
    public static WebApplication UseApplication(this WebApplication app)
    {
        app.SeedDatabase();
        app.UseGlobalExceptionHandling();
        app.UseSwaggerDocumentation(DependencyInjection.WebOptions.SwaggerUiTitle);
        app.UseCorsPolicy(DependencyInjection.WebOptions.CorsAppliedPolicyName ?? DependencyInjection.WebOptions.CorsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapEndpoints();
        app.UseMiddlewares();

        return app;
    }

    private static WebApplication SeedDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<PaymentSeeder>();
        seeder.SeedAsync().GetAwaiter().GetResult();

        return app;
    }

    private static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapCarter();
        app.MapHealthChecks("/health");
        return app;
    }

    private static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<RequestContextMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequiredHeadersMiddleware>();
        app.UseIdempotency();
        return app;
    }
}
