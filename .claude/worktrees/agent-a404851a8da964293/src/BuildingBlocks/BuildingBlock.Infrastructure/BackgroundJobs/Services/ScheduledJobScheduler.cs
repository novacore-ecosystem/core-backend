using NovaCore.BuildingBlock.Application.Abstractions.Jobs;

using Hangfire;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Services;

public sealed class ScheduledJobScheduler(
    IBackgroundJobClient backgroundJobClient,
    IServiceProvider serviceProvider,
    ILogger<ScheduledJobScheduler> logger) : IScheduledJobScheduler
{
    public string Schedule<TJob>(TimeSpan delay) where TJob : IScheduledJob
    {
        using var scope = serviceProvider.CreateScope();
        var job = scope.ServiceProvider.GetRequiredService<TJob>();

        var jobId = backgroundJobClient.Schedule(
            () => ExecuteJobAsync(typeof(TJob)),
            delay);

        logger.LogInformation(
            "Scheduled job {JobName} (ID: {JobId}) to run after {Delay}ms",
            job.JobName, jobId, delay.TotalMilliseconds);

        return jobId;
    }

    private async Task ExecuteJobAsync(Type jobType)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var job = (IScheduledJob)scope.ServiceProvider.GetRequiredService(jobType);
            logger.LogInformation("Executing scheduled job: {JobName}", job.JobName);
            await job.ExecuteAsync();
            logger.LogInformation("Scheduled job completed: {JobName}", job.JobName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scheduled job failed: {JobType}", jobType.Name);
            throw;
        }
    }
}
