using NovaCore.BuildingBlock.Web.Cors;
using NovaCore.BuildingBlock.Web.Middleware;
using NovaCore.BuildingBlock.Web.Swagger;

using NovaCore.BuildingBlock.Web.ExceptionHandling;
using NovaCore.Notification.Infrastructure.BackgroundJobs;
using NovaCore.Notification.Infrastructure.SignalR.Hubs.Global;

namespace NovaCore.Notification.API;

public static class ApplicationPipeline
{
    public static WebApplication UseApplication(this WebApplication app)
    {
        app.UseBackgroundJobsDashboard();
        app.UseBackgroundJobsScheduling();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseGlobalExceptionHandling()
            .UseSwaggerDocumentation(DependencyInjection.WebOptions.SwaggerUiTitle)
            .UseCorsPolicy(DependencyInjection.WebOptions.CorsAppliedPolicyName ?? DependencyInjection.WebOptions.CorsPolicyName)
            .MapEndpoints()
            .UseMiddlewares()
            .MapHubs();

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
        app.UseMiddleware<RequiredHeadersMiddleware>();
        return app;
    }

    private static WebApplication MapHubs(this WebApplication app)
    {
        app.MapHub<GlobalHub>(GlobalHub.Path);
        return app;
    }
}
