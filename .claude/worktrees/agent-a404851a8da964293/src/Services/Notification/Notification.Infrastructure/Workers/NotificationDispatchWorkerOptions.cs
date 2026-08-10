using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Notification.Infrastructure.Workers;

public sealed class NotificationDispatchWorkerOptions : IJobOptions
{
    public const string Section = "Jobs:DispatchWorker";

    public string JobId { get; set; } = "notification-dispatch-worker";
    public string CronExpression { get; set; } = "*/1 * * * *";
    public string Queue { get; set; } = JobQueueConstant.DEFAULT;
    public bool IsInit { get; set; }

    /// <summary>Whether the job actually polls anything when it runs. Off = no-op, cron stays registered.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Dispatches fetched per run.</summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>Attempts (including the first) before a dispatch is dead-lettered instead of retried.</summary>
    public int MaxRetryCount { get; set; } = 5;

    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(10);

    public double RetryBackoffMultiplier { get; set; } = 2.0;

    public TimeSpan MaximumRetryDelay { get; set; } = TimeSpan.FromMinutes(30);
}
