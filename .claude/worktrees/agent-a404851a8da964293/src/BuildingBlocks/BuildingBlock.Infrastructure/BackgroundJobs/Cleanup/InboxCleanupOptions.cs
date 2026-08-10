using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;

public sealed class InboxCleanupOptions : IJobOptions
{
    public const string Section = "Jobs:InboxCleanup";

    public string JobId { get; set; } = "inbox-cleanup";
    public string CronExpression { get; set; } = "0 3 * * *";
    public string Queue { get; set; } = JobQueueConstant.DEFAULT;
    public bool IsInit { get; set; }

    /// <summary>Whether the job actually deletes anything when it runs. Off = no-op, cron stays registered.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Dedup rows older than this are eligible for deletion.</summary>
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(7);

    /// <summary>Rows deleted per DELETE statement.</summary>
    public int BatchSize { get; set; } = 500;

    /// <summary>Safety cap on batches per run, so one execution can't hold a long-running job slot indefinitely - any remainder is picked up on the next scheduled run.</summary>
    public int MaxBatchesPerRun { get; set; } = 20;
}
