using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Hangfire;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Services;

/// <summary>
/// Discovers every loaded IRecurringJob implementation and registers it with Hangfire.
/// Shared across services - a job only needs to be registered in DI (via
/// AddScopedByInterfaceAndConcrete&lt;IRecurringJob&gt;) to be picked up here.
/// </summary>
public sealed class RecurringJobRegistry(
    IServiceProvider serviceProvider,
    IAppLogger<RecurringJobRegistry> logger)
{
    public void RegisterRecurringJobs(IApplicationBuilder app)
    {
        var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();
        var backgroundJobClient = app.ApplicationServices.GetRequiredService<IBackgroundJobClient>();

        var jobTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IRecurringJob).IsAssignableFrom(p) && !p.IsInterface);

        foreach (var jobType in jobTypes)
        {
            try
            {
                var scope = serviceProvider.CreateScope();
                var job = (IRecurringJob)scope.ServiceProvider.GetRequiredService(jobType);

                recurringJobManager.AddOrUpdate(
                    job.JobId,
                    () => job.ExecuteAsync(CancellationToken.None),
                    job.CronExpression,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.Utc,
                        MisfireHandling = MisfireHandlingMode.Relaxed
                    });

                logger.Information(
                    "Registered recurring job: {JobId} (Cron: {Cron}, Queue: {Queue})",
                    job.JobId, job.CronExpression, job.Queue);

                if (job.IsInit)
                {
                    backgroundJobClient.Enqueue(() => job.ExecuteAsync(CancellationToken.None));
                    logger.Information("Enqueued immediate execution for job: {JobId}", job.JobId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to register job {JobType}", jobType.Name);
            }
        }

        logger.Information("Recurring job registration completed");
    }
}
