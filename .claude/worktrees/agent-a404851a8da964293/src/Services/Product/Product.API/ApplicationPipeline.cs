using NovaCore.BuildingBlock.Infrastructure.Extensions;
using NovaCore.BuildingBlock.Web.Cors;
using NovaCore.BuildingBlock.Web.Middleware;
using NovaCore.BuildingBlock.Web.Swagger;

using NovaCore.BuildingBlock.Web.ExceptionHandling;
using NovaCore.Product.Infrastructure.BackgroundJobs;
using NovaCore.Product.Persistence.Engine;
using NovaCore.Product.Persistence.Storage.Seeders;

namespace NovaCore.Product.API;

public static class ApplicationPipeline
{
    public static WebApplication UseApplication(this WebApplication app)
    {
        app.SeedDatabase();
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
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            var seeder = new ProductSeeder(context);
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
        app.UseIdempotency();
        return app;
    }
}
