using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Web.ExceptionHandling;

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    public static WebApplication UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler();

        return app;
    }
}
