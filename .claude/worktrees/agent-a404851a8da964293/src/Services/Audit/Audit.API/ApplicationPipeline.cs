using NovaCore.BuildingBlock.Web.Cors;
using NovaCore.BuildingBlock.Web.Middleware;
using NovaCore.BuildingBlock.Web.Swagger;

using NovaCore.BuildingBlock.Web.ExceptionHandling;
using NovaCore.Audit.Infrastructure.BackgroundJobs;

namespace NovaCore.Audit.API;

public static class ApplicationPipeline
{
    public static WebApplication UseApplication(this WebApplication app)
    {
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

    private static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapCarter();
        app.MapHealthChecks("/health");
        return app;
    }

    private static WebApplication UseMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<RequestContextMiddleware>();
        return app;
    }
}
