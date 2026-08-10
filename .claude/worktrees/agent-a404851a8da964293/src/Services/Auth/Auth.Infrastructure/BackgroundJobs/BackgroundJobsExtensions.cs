using NovaCore.Auth.Infrastructure.BackgroundJobs.Jobs.RefreshTokenSync;

using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.Auth.Infrastructure.BackgroundJobs;

public static class BackgroundJobsExtensions
{
    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .Configure<RefreshTokenJobOptions>(configuration.GetSection("Jobs:RefreshTokenSync"))
            .AddHangfireScheduling(configuration, typeof(BackgroundJobsExtensions));

        return services;
    }

    public static void UseBackgroundJobsDashboard(this IApplicationBuilder app)
    {
        app.UseHangfireJobsDashboard("Auth Service - Background Jobs");
    }

    public static void UseBackgroundJobsScheduling(this IApplicationBuilder app)
    {
        app.UseHangfireScheduling();
    }
}
