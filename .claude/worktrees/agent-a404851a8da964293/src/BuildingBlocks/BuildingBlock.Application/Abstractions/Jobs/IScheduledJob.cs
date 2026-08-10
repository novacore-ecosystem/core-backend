namespace NovaCore.BuildingBlock.Application.Abstractions.Jobs;

/// <summary>
/// Defines a one-time scheduled job that runs after a delay.
/// Implement this interface for any delayed background job.
///
/// Features:
/// - Custom delay scheduling
/// - Auto-discovery via ScheduledJobScheduler
/// - Hangfire dashboard monitoring
/// - Job ID tracking for future reference
///
/// Usage:
/// <code>
/// public sealed class MyScheduledJob : IScheduledJob
/// {
///     public string JobId => "my-scheduled-job";
///     public string JobName => "My Scheduled Job";
///
///     public async Task ExecuteAsync(CancellationToken ct)
///     {
///         // Your job logic
///     }
/// }
///
/// // Schedule the job:
/// var scheduler = serviceProvider.GetRequiredService&lt;IScheduledJobScheduler&gt;();
/// var jobId = scheduler.Schedule&lt;MyScheduledJob&gt;(TimeSpan.FromHours(2));
/// </code>
/// </summary>
public interface IScheduledJob
{
    /// <summary>Unique job identifier (used as Hangfire job ID)</summary>
    string JobId { get; }

    /// <summary>Human-readable job name for logging and dashboard</summary>
    string JobName { get; }

    /// <summary>
    /// Execute the job logic.
    /// Called by Hangfire scheduler after the specified delay.
    /// </summary>
    /// <param name="ct">Cancellation token for graceful shutdown</param>
    /// <remarks>
    /// Do not throw unhandled exceptions. Log errors internally or throw for Hangfire to retry.
    /// </remarks>
    Task ExecuteAsync(CancellationToken ct = default);
}
