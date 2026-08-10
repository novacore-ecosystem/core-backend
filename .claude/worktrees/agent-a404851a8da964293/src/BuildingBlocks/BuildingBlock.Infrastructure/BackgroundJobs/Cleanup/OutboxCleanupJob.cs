using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;

/// <summary>
/// Recurring job that deletes obsolete, already-published Outbox rows in batches.
/// Invokes only the IOutboxStore persistence contract - the actual DELETE lives in the
/// persistence provider (e.g. EfOutboxStore), never here.
/// </summary>
public sealed class OutboxCleanupJob(
    IOutboxStore outboxStore,
    IOptions<OutboxCleanupOptions> options,
    IAppLogger<OutboxCleanupJob> logger) : IRecurringJob
{
    private readonly OutboxCleanupOptions _options = options.Value;

    public string JobId => _options.JobId;
    public string CronExpression => _options.CronExpression;
    public string Queue => _options.Queue;
    public bool IsInit => _options.IsInit;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            logger.Information("Outbox cleanup is disabled, skipping run");
            return;
        }

        var cutoffUtc = DateTime.UtcNow - _options.RetentionPeriod;
        var totalDeleted = 0;

        try
        {
            for (var batch = 0; batch < _options.MaxBatchesPerRun; batch++)
            {
                var deleted = await outboxStore.DeleteProcessedBeforeAsync(cutoffUtc, _options.BatchSize, cancellationToken);
                totalDeleted += deleted;

                if (deleted < _options.BatchSize)
                    break;
            }

            logger.Information(
                "Outbox cleanup completed. Deleted {Count} processed message(s) older than {CutoffUtc}",
                totalDeleted, cutoffUtc);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Outbox cleanup failed after deleting {Count} message(s); remainder will be retried on the next scheduled run", totalDeleted);
        }
    }
}
