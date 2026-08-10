using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.User.Infrastructure.BackgroundJobs;

public static class BackgroundJobsExtensions
{
    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfireScheduling(configuration, typeof(BackgroundJobsExtensions));
        return services;
    }

    public static void UseBackgroundJobsDashboard(this IApplicationBuilder app)
    {
        app.UseHangfireJobsDashboard("User Service - Background Jobs");
    }

    public static void UseBackgroundJobsScheduling(this IApplicationBuilder app)
    {
        app.UseHangfireScheduling();
    }
}
