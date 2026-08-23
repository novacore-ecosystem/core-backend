using NovaCore.BuildingBlock.Web.Cors;
using NovaCore.BuildingBlock.Web.ExceptionHandling;
using NovaCore.BuildingBlock.Web.Middleware;
using NovaCore.BuildingBlock.Web.Swagger;

using NovaCore.Content.Infrastructure.BackgroundJobs;
using NovaCore.Content.Persistence.Engine;
using NovaCore.Content.Persistence.Storage.Seeders;

namespace NovaCore.Content.API;

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
        app.UseBackgroundJobsDashboard();
        app.UseBackgroundJobsScheduling();

        return app;
    }

    private static void SeedDatabase(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
            var seeder = new ContentSeeder(context);
            seeder.SeedAsync().Wait();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while seeding the database");
            if (app.Environment.IsDevelopment())
                throw;
        }
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
        app.UseMiddleware<RequiredHeadersMiddleware>();
        return app;
    }
}
