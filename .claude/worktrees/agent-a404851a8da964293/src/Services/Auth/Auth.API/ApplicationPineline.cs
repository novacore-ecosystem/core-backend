using NovaCore.BuildingBlock.Web.ExceptionHandling;
using NovaCore.Auth.API.GrpcServices;
using NovaCore.Auth.Application.Abstractions.Services;
using NovaCore.Auth.Infrastructure.BackgroundJobs;
using NovaCore.Auth.Persistence.Storage.Seeders;

using NovaCore.BuildingBlock.Web.Cors;
using NovaCore.BuildingBlock.Web.Middleware;
using NovaCore.BuildingBlock.Web.Swagger;

namespace NovaCore.Auth.API;

public static class ApplicationPipeline
{
    public static WebApplication UseApplication(this WebApplication app)
    {
        app.SeedDatabase();
        app.InitializeRefreshTokenCache();
        app.UseBackgroundJobsDashboard();
        app.UseBackgroundJobsScheduling();
        app.UseGlobalExceptionHandling();
        app.UseSwaggerDocumentation(DependencyInjection.WebOptions.SwaggerUiTitle);
        app.UseCorsPolicy(DependencyInjection.WebOptions.CorsAppliedPolicyName ?? DependencyInjection.WebOptions.CorsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapEndpoints();
        app.UseMiddlewares();

        return app;
    }

    private static void SeedDatabase(this WebApplication app)
    {
        try
        {
            app.Services.SeedDatabaseAsync().Wait();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while seeding the database");
            if (app.Environment.IsDevelopment())
                throw;
        }
    }

    private static void InitializeRefreshTokenCache(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var initService = scope.ServiceProvider.GetRequiredService<IRefreshTokenInitializationService>();
            initService.InitializeAsync(CancellationToken.None).Wait();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while initializing refresh token cache");
            if (app.Environment.IsDevelopment())
                throw;
        }
    }

    private static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapCarter();
        app.MapGrpcService<AuthGrpcServiceImpl>();
        app.MapHealthChecks("/health");
        return app;
    }

    private static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<RequestContextMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequiredHeadersMiddleware>();
        return app;
    }
}
