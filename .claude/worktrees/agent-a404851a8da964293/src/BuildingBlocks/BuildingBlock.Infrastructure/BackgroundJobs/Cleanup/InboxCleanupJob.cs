using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;

/// <summary>
/// Recurring job that deletes obsolete Inbox dedup rows in batches. Invokes only the
/// IInboxStore persistence contract - the actual DELETE lives in the persistence
/// provider (e.g. EfInboxStore), never here.
/// </summary>
public sealed class InboxCleanupJob(
    IInboxStore inboxStore,
    IOptions<InboxCleanupOptions> options,
    IAppLogger<InboxCleanupJob> logger) : IRecurringJob
{
    private readonly InboxCleanupOptions _options = options.Value;

    public string JobId => _options.JobId;
    public string CronExpression => _options.CronExpression;
    public string Queue => _options.Queue;
    public bool IsInit => _options.IsInit;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            logger.Information("Inbox cleanup is disabled, skipping run");
            return;
        }

        var cutoffUtc = DateTime.UtcNow - _options.RetentionPeriod;
        var totalDeleted = 0;

        try
        {
            for (var batch = 0; batch < _options.MaxBatchesPerRun; batch++)
            {
                var deleted = await inboxStore.DeleteProcessedBeforeAsync(cutoffUtc, _options.BatchSize, cancellationToken);
                totalDeleted += deleted;

                if (deleted < _options.BatchSize)
                    break;
            }

            logger.Information(
                "Inbox cleanup completed. Deleted {Count} dedup row(s) older than {CutoffUtc}",
                totalDeleted, cutoffUtc);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Inbox cleanup failed after deleting {Count} row(s); remainder will be retried on the next scheduled run", totalDeleted);
        }
    }
}
