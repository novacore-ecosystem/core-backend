using NovaCore.BuildingBlock.Infrastructure.BackgroundJobs;
using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Notification.Infrastructure.Configurations.Settings;

/// <summary>Hangfire schedule, retry/backoff, and batching knobs for the NotificationDispatch queue drain worker.</summary>
public sealed class NotificationSchedulerSetting : SchedulerSettingBase
{
    public const string Section = "Jobs:DispatchWorker";

    public override string JobId { get; set; } = "notification-dispatch-worker";
    public override string CronExpression { get; set; } = "*/1 * * * *";
    public override string Queue { get; set; } = JobQueueConstant.DEFAULT;
    public override bool IsInit { get; set; } = false;

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
