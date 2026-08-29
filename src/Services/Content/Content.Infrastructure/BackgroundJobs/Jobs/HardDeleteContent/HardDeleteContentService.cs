using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Infrastructure.Configurations.Settings;

using Microsoft.Extensions.Options;

namespace NovaCore.Content.Infrastructure.BackgroundJobs.Jobs.HardDeleteContent;

/// <summary>
/// Permanently removes Content rows that have been soft-deleted for longer than the configured
/// retention period (business default 7 days - see WCM spec section 12.3). Cascading FKs
/// (content_versions, content_localizations, content_publications, ... all Cascade on ContentId)
/// take care of every dependent row once the root Content is removed, so this job only ever
/// touches the aggregate root.
/// </summary>
public sealed class HardDeleteContentService(
    IContentReadService contentReadService,
    IContentWriteService contentWriteService,
    IUnitOfWork unitOfWork,
    IAppLogger<HardDeleteContentService> logger,
    IOptions<ContentSchedulerSetting> options) : IRecurringJob
{
    public string JobId => options.Value.JobId;
    public string CronExpression => options.Value.CronExpression;
    public string Queue => options.Value.Queue;
    public bool IsInit => options.Value.IsInit;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.UtcNow.AddDays(-Math.Max(1, options.Value.RetentionDays));
        var batchSize = Math.Max(1, options.Value.BatchSize);
        var totalDeleted = 0;

        while (true)
        {
            var ids = await contentReadService.GetHardDeleteEligibleIdsAsync(threshold, batchSize, cancellationToken);
            if (ids.Count == 0)
                break;

            await unitOfWork.ExecuteTransactionAsync(async () =>
            {
                foreach (var id in ids)
                    await contentWriteService.HardDeleteAsync(id, cancellationToken);
            }, ct: cancellationToken);

            totalDeleted += ids.Count;

            // A batch smaller than batchSize means we've exhausted the eligible backlog for this run.
            if (ids.Count < batchSize)
                break;
        }

        if (totalDeleted > 0)
            logger.Information("Hard-deleted {Count} Content items past the {RetentionDays}-day retention threshold", totalDeleted, options.Value.RetentionDays);
    }
}
