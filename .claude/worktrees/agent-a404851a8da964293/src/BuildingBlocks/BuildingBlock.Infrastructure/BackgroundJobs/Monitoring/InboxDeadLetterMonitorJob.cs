using NovaCore.BuildingBlock.Application.Abstractions.Jobs;
using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using Microsoft.Extensions.Options;

namespace NovaCore.BuildingBlock.Infrastructure.BackgroundJobs.Monitoring;

/// <summary>
/// Recurring job that surfaces Inbox rows stuck in DeadLetter - today the only signal that a
/// message needs operator attention, since dead-lettered rows are never retried automatically
/// (see InboxAttemptExecutor). Logs a warning per (consumer, topic) group so the count is
/// visible to whatever aggregates application logs, instead of requiring someone to know to
/// query the Inbox table.
/// </summary>
public sealed class InboxDeadLetterMonitorJob(
    IInboxStore inboxStore,
    IOptions<InboxDeadLetterMonitorOptions> options,
    IAppLogger<InboxDeadLetterMonitorJob> logger) : IRecurringJob
{
    private readonly InboxDeadLetterMonitorOptions _options = options.Value;

    public string JobId => _options.JobId;
    public string CronExpression => _options.CronExpression;
    public string Queue => _options.Queue;
    public bool IsInit => _options.IsInit;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            logger.Information("Inbox dead-letter monitor is disabled, skipping run");
            return;
        }

        var summary = await inboxStore.GetDeadLetterSummaryAsync(cancellationToken);

        if (summary.Count == 0)
        {
            logger.Debug("Inbox dead-letter monitor: no dead-lettered messages");
            return;
        }

        foreach (var group in summary)
        {
            logger.Warning(
                "Inbox has {Count} dead-lettered message(s) for consumer {ConsumerName} on topic {Topic}, oldest since {OldestDeadLetteredAt:O} - not retried automatically, requires manual replay or intervention",
                group.Count, group.ConsumerName, group.Topic, group.OldestDeadLetteredAt);
        }
    }
}
