using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Microsoft.Extensions.Options;

using NovaCore.Notification.Application.Abstractions.Persistence.NotificationDispatches;
using NovaCore.Notification.Application.Abstractions.Services;
using NovaCore.Notification.Domain.Entities;
using NovaCore.Notification.Domain.Enums;

namespace NovaCore.Notification.Infrastructure.Workers;

/// <summary>
/// Recurring job draining the NotificationDispatch queue (Pending, or Failed rows whose
/// NextRetryAt has elapsed) - the Outbox-style delivery worker for the "Delivery" half of this
/// service. For each due row: MarkProcessing, resolve the channel's sender (bailing out early if
/// the channel is missing/disabled in NotificationChannel), attempt delivery, then MarkSent or
/// MarkFailed with an exponentially-backed-off retry time - dead-lettering once MaxRetryCount is
/// exhausted, mirroring Inbox's retry/backoff shape (see Inbox:Retry in appsettings.json).
/// </summary>
public sealed class NotificationDispatchWorker(
    INotificationDispatchReadService dispatchReadService,
    INotificationDispatchWriteService dispatchWriteService,
    INotificationChannelCache channelCache,
    IChannelSenderResolver senderResolver,
    IOptions<NotificationDispatchWorkerOptions> options,
    IAppLogger<NotificationDispatchWorker> logger) : IRecurringJob
{
    private readonly NotificationDispatchWorkerOptions _options = options.Value;

    public string JobId => _options.JobId;
    public string CronExpression => _options.CronExpression;
    public string Queue => _options.Queue;
    public bool IsInit => _options.IsInit;

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            logger.Information("Notification dispatch worker is disabled, skipping run");
            return;
        }

        var due = await dispatchReadService.GetDueForProcessingAsync(_options.BatchSize, ct);
        if (due.Count == 0)
            return;

        logger.Information("Notification dispatch worker found {Count} dispatch(es) due for processing", due.Count);

        foreach (var dispatch in due)
        {
            await ProcessAsync(dispatch, ct);
        }
    }

    private async Task ProcessAsync(NotificationDispatch dispatch, CancellationToken ct)
    {
        dispatch.MarkProcessing();
        await dispatchWriteService.UpdateAsync(dispatch, ct);

        try
        {
            var channel = await channelCache.GetByChannelTypeAsync(dispatch.Channel, ct);
            if (channel is null || channel.Status != NotificationChannelStatus.Active)
                throw new InvalidOperationException($"Channel {dispatch.Channel} is not configured or is disabled.");

            var sender = senderResolver.Resolve(dispatch.Channel);
            await sender.SendAsync(dispatch, ct);

            dispatch.MarkSent(DateTime.UtcNow);
            logger.Information("Dispatch {DispatchId} sent via {Channel}", dispatch.Id, dispatch.Channel);
        }
        catch (Exception ex)
        {
            var nextRetryAtUtc = dispatch.RetryCount + 1 >= _options.MaxRetryCount
                ? (DateTime?)null
                : DateTime.UtcNow.Add(ComputeBackoff(dispatch.RetryCount));

            dispatch.MarkFailed(ex.Message, nextRetryAtUtc);
            logger.Error(ex, "Dispatch {DispatchId} on channel {Channel} failed", dispatch.Id, dispatch.Channel);
        }

        await dispatchWriteService.UpdateAsync(dispatch, ct);
    }

    private TimeSpan ComputeBackoff(int retryCount)
    {
        var delay = _options.InitialRetryDelay * Math.Pow(_options.RetryBackoffMultiplier, retryCount);
        return delay > _options.MaximumRetryDelay ? _options.MaximumRetryDelay : delay;
    }
}
