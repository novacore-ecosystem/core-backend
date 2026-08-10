using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Services;
using NovaCore.BuildingBlock.Infrastructure.Extensions;

using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;

/// <summary>
/// Shared Hangfire bootstrap: storage/server setup, recurring-job discovery and the
/// scheduled-job dispatcher. A service opts in with one call and passes marker types
/// for the assemblies that contain its own IRecurringJob implementations - callers that
/// register jobs from other assemblies (e.g. NovaCore.BuildingBlock.Infrastructure's cleanup jobs)
/// do so through their own DI registration, independent of the markers passed here.
/// </summary>
public static class HangfireSchedulingExtensions
{
    public static IServiceCollection AddHangfireScheduling(
        this IServiceCollection services,
        IConfiguration configuration,
        params Type[] jobAssemblyMarkers)
    {
        services
            .AddScopedByInterfaceAndConcrete<IRecurringJob>(jobAssemblyMarkers)
            .AddScoped<RecurringJobRegistry>()
            .AddScoped<IScheduledJobScheduler, ScheduledJobScheduler>();

        AddHangfireWithPostgres(services, configuration);
        return services;
    }

    public static void UseHangfireJobsDashboard(this IApplicationBuilder app, string dashboardTitle)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            DisplayStorageConnectionString = false,
            DashboardTitle = dashboardTitle,
            IgnoreAntiforgeryToken = true,
            Authorization = [new AllowAllDashboardAuthorizationFilter()]
        });
    }

    private sealed class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext _) => true;
    }

    public static void UseHangfireScheduling(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var registry = scope.ServiceProvider.GetRequiredService<RecurringJobRegistry>();
        registry.RegisterRecurringJobs(app);
    }

    private static void AddHangfireWithPostgres(
        IServiceCollection services,
        IConfiguration configuration)
    {
        // Use dedicated Hangfire connection string if available, otherwise fall back to regular connection
        var connectionString = configuration.GetConnectionString("Hangfire")
            ?? configuration.GetConnectionString("PostgresConnection")
            ?? throw new InvalidOperationException("Hangfire or PostgresConnection not found in configuration");

        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                });
        });

        services.AddHangfireServer(options =>
        {
            options.Queues = ["default", "jobs"];
            options.WorkerCount = Math.Min(Environment.ProcessorCount, 10);
            options.SchedulePollingInterval = TimeSpan.FromSeconds(5);
        });
    }
}
