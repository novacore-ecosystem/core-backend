using NovaCore.BuildingBlock.Persistence.Inbox;

using Microsoft.EntityFrameworkCore;

namespace NovaCore.BuildingBlock.Persistence.Ef.Inbox;

/// <summary>
/// Generic EF implementation of IInboxStore, parameterized over the DbContext type.
/// Derived DbContexts must implement IInboxDbContext to provide access to InboxMessages.
///
/// Scoped lifetime (one instance per Kafka message / per DI scope), same as the DbContext it
/// wraps. BeginAttemptAsync stages the row it looked up (or created) on this instance so the
/// matching CompleteAttemptAsync/FailAttemptAsync call - later in the same scope - can finish
/// mutating that exact tracked entity instead of re-querying it.
/// </summary>
public sealed class EfInboxStore<TContext>(TContext context) : IInboxStore
    where TContext : Microsoft.EntityFrameworkCore.DbContext, IInboxDbContext
{
    private readonly TContext _context = context;

    public async Task<InboxAttemptDecision> BeginAttemptAsync(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        CancellationToken ct = default)
    {
        var existing = await _context.InboxMessages
            .FirstOrDefaultAsync(x => x.MessageId == messageId && x.ConsumerName == consumerName, ct);
        if (existing is not null)
        {
            return existing.Status switch
            {
                InboxMessageStatus.Processed => InboxAttemptDecision.AlreadyProcessed,
                InboxMessageStatus.DeadLetter => InboxAttemptDecision.DeadLettered,
                InboxMessageStatus.Retrying when existing.NextRetryAt > DateTime.UtcNow
                    => InboxAttemptDecision.NotDueYet,
                _ => InboxAttemptDecision.Proceed
            };
        }

        var inbox = InboxMessage.Create(
            messageId,
            consumerName,
            topic,
            payload,
            headersJson);

        await _context.InboxMessages.AddAsync(inbox, ct);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return InboxAttemptDecision.AlreadyProcessed;
        }

        return InboxAttemptDecision.Proceed;
    }

    public async Task CompleteAttemptAsync(
        Guid messageId,
        string consumerName,
        CancellationToken ct = default)
    {
        var inbox = await _context.InboxMessages
            .FirstOrDefaultAsync(x => x.MessageId == messageId && x.ConsumerName == consumerName, ct);
        if (inbox is null)
            return;

        inbox.MarkProcessed();
        await CloseOpenRetryHistoryAsync(inbox.Id, InboxRetryHistoryResult.Succeeded, null, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<InboxFailureOutcome> FailAttemptAsync(
        Guid messageId,
        string consumerName,
        string error,
        InboxRetryPolicy policy,
        CancellationToken ct = default)
    {
        var inbox = await _context.InboxMessages
            .FirstOrDefaultAsync(x => x.MessageId == messageId && x.ConsumerName == consumerName, ct);
        if (inbox is null)
            return InboxFailureOutcome.WillRetry;

        inbox.MarkFailed(error, policy);
        await CloseOpenRetryHistoryAsync(inbox.Id, InboxRetryHistoryResult.FailedAgain, error, ct);

        await _context.SaveChangesAsync(ct);

        return inbox.Status == InboxMessageStatus.DeadLetter
            ? InboxFailureOutcome.DeadLettered
            : InboxFailureOutcome.WillRetry;
    }

    /// <summary>
    /// Closes the single open (FinishedAt == null) retry-history entry for this row, if any. A
    /// no-op indexed lookup for the vast majority of messages that were never manually retried.
    /// </summary>
    private async Task CloseOpenRetryHistoryAsync(
        Guid inboxMessageId, InboxRetryHistoryResult result, string? exception, CancellationToken ct)
    {
        var open = await _context.InboxRetryHistories
            .FirstOrDefaultAsync(h => h.InboxMessageId == inboxMessageId && h.FinishedAt == null, ct);

        open?.Close(result, exception);
    }

    public async Task<IReadOnlyList<InboxMessageSnapshot>> GetDueForRetryAsync(int batchSize, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var messages = await _context.InboxMessages
            .AsNoTracking()
            .Where(m => m.Status == InboxMessageStatus.Retrying && m.NextRetryAt <= now)
            .OrderBy(m => m.NextRetryAt)
            .Take(batchSize)
            .ToListAsync(ct);

        return [.. messages.Select(ToSnapshot)];
    }

    public async Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default)
    {
        var ids = await _context.InboxMessages
            .Where(m => m.Status == InboxMessageStatus.Processed && m.ProcessedAt < olderThanUtc)
            .OrderBy(m => m.ProcessedAt)
            .Take(batchSize)
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (ids.Count == 0)
            return 0;

        await _context.InboxMessages
            .Where(m => ids.Contains(m.Id))
            .ExecuteDeleteAsync(ct);

        return ids.Count;
    }

    public async Task<IReadOnlyList<InboxDeadLetterSummary>> GetDeadLetterSummaryAsync(CancellationToken ct = default)
    {
        var groups = await _context.InboxMessages
            .AsNoTracking()
            .Where(m => m.Status == InboxMessageStatus.DeadLetter)
            .GroupBy(m => new { m.ConsumerName, m.Topic })
            .Select(g => new
            {
                g.Key.ConsumerName,
                g.Key.Topic,
                Count = g.Count(),
                OldestDeadLetteredAt = g.Min(m => m.LastRetryAt)
            })
            .ToListAsync(ct);

        return [.. groups.Select(g => new InboxDeadLetterSummary(
            g.ConsumerName, g.Topic, g.Count, g.OldestDeadLetteredAt ?? DateTime.UtcNow))];
    }

    public async Task<InboxRequeueResult> RequeueDeadLetterAsync(
        Guid inboxMessageId, string? operatorId, CancellationToken ct = default)
    {
        var inbox = await _context.InboxMessages
            .FirstOrDefaultAsync(x => x.Id == inboxMessageId, ct);

        if (inbox is null)
            return new InboxRequeueResult(InboxRequeueOutcome.NotFound, null, 0);

        if (inbox.Status != InboxMessageStatus.DeadLetter)
            return new InboxRequeueResult(InboxRequeueOutcome.NotDeadLetter, null, 0);

        // Conditional update: only succeeds if the row is still DeadLetter at the moment of the
        // write, so two concurrent requeue calls for the same id can never both win.
        var rowsAffected = await _context.InboxMessages
            .Where(x => x.Id == inboxMessageId && x.Status == InboxMessageStatus.DeadLetter)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, InboxMessageStatus.Retrying)
                .SetProperty(x => x.NextRetryAt, (DateTime?)null)
                .SetProperty(x => x.RetryCount, 0), ct);

        if (rowsAffected == 0)
            return new InboxRequeueResult(InboxRequeueOutcome.NotDeadLetter, null, 0);

        var priorRetries = await _context.InboxRetryHistories
            .CountAsync(h => h.InboxMessageId == inboxMessageId, ct);
        var retryNumber = priorRetries + 1;

        var history = InboxRetryHistory.Start(
            inboxMessageId, inbox.MessageId, inbox.ConsumerName, inbox.Topic, retryNumber, operatorId);
        await _context.InboxRetryHistories.AddAsync(history, ct);
        await _context.SaveChangesAsync(ct);

        var snapshot = ToSnapshot(inbox) with { Status = InboxMessageStatus.Retrying, NextRetryAt = null, RetryCount = 0 };
        return new InboxRequeueResult(InboxRequeueOutcome.Requeued, snapshot, retryNumber);
    }

    public async Task RevertFailedRequeueAsync(Guid inboxMessageId, string error, CancellationToken ct = default)
    {
        await _context.InboxMessages
            .Where(x => x.Id == inboxMessageId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, InboxMessageStatus.DeadLetter), ct);

        await CloseOpenRetryHistoryAsync(inboxMessageId, InboxRetryHistoryResult.FailedAgain, error, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<InboxRetryHistoryEntry>> GetRetryHistoryAsync(
        Guid inboxMessageId, CancellationToken ct = default)
    {
        var rows = await _context.InboxRetryHistories
            .AsNoTracking()
            .Where(h => h.InboxMessageId == inboxMessageId)
            .OrderByDescending(h => h.StartedAt)
            .ToListAsync(ct);

        return [.. rows.Select(h => new InboxRetryHistoryEntry(
            h.Id, h.InboxMessageId, h.MessageId, h.ConsumerName, h.Topic, h.RetryNumber,
            h.StartedAt, h.FinishedAt, h.DurationMs, h.Operator, h.Result, h.Exception))];
    }

    private static InboxMessageSnapshot ToSnapshot(InboxMessage m) => new(
        m.MessageId,
        m.ConsumerName,
        m.Topic,
        m.Payload,
        m.HeadersJson,
        m.Status,
        m.RetryCount,
        m.CreatedAt,
        m.ProcessedAt,
        m.NextRetryAt,
        m.LastRetryAt,
        m.LastError);
}
