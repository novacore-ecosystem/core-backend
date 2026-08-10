using NovaCore.BuildingBlock.Persistence.Inbox;
using NovaCore.BuildingBlock.Persistence.Mongo.MongoContext;

using MongoDB.Driver;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Inbox;

/// <summary>
/// Generic Mongo implementation of IInboxStore, parameterized over the Mongo context type.
/// Derived contexts must implement IInboxMongoContext to provide access to InboxMessages.
///
/// Unlike EfInboxStore, there is no shared change tracker to stage an optimistic completion
/// marker into: NovaCore.Audit.Persistence.Engine.UnitOfWork already documents that Mongo writes commit
/// immediately per call, with no SaveChanges to flush. CompleteAttemptAsync therefore writes the
/// Processed status directly once the handler returns, the same small window that existed before
/// this change - InboxFailureOutcome.AlreadyCommitted (the EF provider's atomic-commit detection)
/// never applies here.
/// </summary>
public sealed class MongoInboxStore<TContext>(TContext context) : IInboxStore
    where TContext : MongoContextBase, IInboxMongoContext
{
    private readonly TContext _context = context;
    private Guid? _currentAttemptId;

    public async Task<InboxAttemptDecision> BeginAttemptAsync(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson,
        CancellationToken ct = default)
    {
        var existing = await _context.InboxMessages
            .Find(x => x.MessageId == messageId && x.ConsumerName == consumerName)
            .FirstOrDefaultAsync(ct);
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

        var document = InboxDocument.Create(
            messageId,
            consumerName,
            topic,
            payload,
            headersJson);

        try
        {
            await _context.InboxMessages.InsertOneAsync(document, cancellationToken: ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
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
        var doc = await _context.InboxMessages
            .Find(x => x.MessageId == messageId && x.ConsumerName == consumerName)
            .FirstOrDefaultAsync(ct);
        if (doc is null)
            return;

        var update = Builders<InboxDocument>.Update
            .Set(x => x.Status, InboxMessageStatus.Processed)
            .Set(x => x.ProcessedAt, DateTime.UtcNow)
            .Set(x => x.NextRetryAt, null)
            .Set(x => x.LastError, null);

        await _context.InboxMessages
            .UpdateOneAsync(x => x.MessageId == messageId && x.ConsumerName == consumerName,
            update,
            cancellationToken: ct);

        await CloseOpenRetryHistoryAsync(doc.Id, InboxRetryHistoryResult.Succeeded, null, ct);
    }

    public async Task<InboxFailureOutcome> FailAttemptAsync(
        Guid messageId,
        string consumerName,
        string error,
        InboxRetryPolicy policy,
        CancellationToken ct = default)
    {
        var doc = await _context.InboxMessages
            .Find(x => x.MessageId == messageId && x.ConsumerName == consumerName)
            .FirstOrDefaultAsync(ct);
        if (doc is null)
            return InboxFailureOutcome.WillRetry;

        doc.MarkFailed(error, policy);

        await _context.InboxMessages.ReplaceOneAsync(
            x => x.Id == doc.Id,
            doc,
            cancellationToken: ct);

        await CloseOpenRetryHistoryAsync(doc.Id, InboxRetryHistoryResult.FailedAgain, error, ct);

        return doc.Status == InboxMessageStatus.DeadLetter
            ? InboxFailureOutcome.DeadLettered
            : InboxFailureOutcome.WillRetry;
    }

    /// <summary>
    /// Closes the single open (FinishedAt == null) retry-history entry for this row, if any. A
    /// no-op lookup for the vast majority of messages that were never manually retried.
    /// </summary>
    private async Task CloseOpenRetryHistoryAsync(
        Guid inboxMessageId, InboxRetryHistoryResult result, string? exception, CancellationToken ct)
    {
        var open = await _context.InboxRetryHistories
            .Find(h => h.InboxMessageId == inboxMessageId && h.FinishedAt == null)
            .FirstOrDefaultAsync(ct);
        if (open is null)
            return;

        open.Close(result, exception);

        await _context.InboxRetryHistories.ReplaceOneAsync(h => h.Id == open.Id, open, cancellationToken: ct);
    }

    public async Task<IReadOnlyList<InboxMessageSnapshot>> GetDueForRetryAsync(int batchSize, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var docs = await _context.InboxMessages
            .Find(m => m.Status == InboxMessageStatus.Retrying && m.NextRetryAt <= now)
            .SortBy(m => m.NextRetryAt)
            .Limit(batchSize)
            .ToListAsync(ct);

        return [.. docs.Select(ToSnapshot)];
    }

    public async Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default)
    {
        var ids = await _context.InboxMessages
            .Find(m => m.Status == InboxMessageStatus.Processed && m.ProcessedAt < olderThanUtc)
            .SortBy(m => m.ProcessedAt)
            .Limit(batchSize)
            .Project(m => m.Id)
            .ToListAsync(ct);

        if (ids.Count == 0)
            return 0;

        var result = await _context.InboxMessages.DeleteManyAsync(
            Builders<InboxDocument>.Filter.In(m => m.Id, ids), ct);

        return (int)result.DeletedCount;
    }

    public async Task<IReadOnlyList<InboxDeadLetterSummary>> GetDeadLetterSummaryAsync(CancellationToken ct = default)
    {
        var docs = await _context.InboxMessages
            .Find(m => m.Status == InboxMessageStatus.DeadLetter)
            .Project(m => new { m.ConsumerName, m.Topic, m.LastRetryAt })
            .ToListAsync(ct);

        return [.. docs
            .GroupBy(d => (d.ConsumerName, d.Topic))
            .Select(g => new InboxDeadLetterSummary(
                g.Key.ConsumerName, g.Key.Topic, g.Count(), g.Min(d => d.LastRetryAt) ?? DateTime.UtcNow))];
    }

    public async Task<InboxRequeueResult> RequeueDeadLetterAsync(
        Guid inboxMessageId, string? operatorId, CancellationToken ct = default)
    {
        var doc = await _context.InboxMessages
            .Find(x => x.Id == inboxMessageId)
            .FirstOrDefaultAsync(ct);

        if (doc is null)
            return new InboxRequeueResult(InboxRequeueOutcome.NotFound, null, 0);

        if (doc.Status != InboxMessageStatus.DeadLetter)
            return new InboxRequeueResult(InboxRequeueOutcome.NotDeadLetter, null, 0);

        // Conditional update: only matches if the doc is still DeadLetter at write time, so two
        // concurrent requeue calls for the same id can never both report Requeued.
        var update = Builders<InboxDocument>.Update
            .Set(x => x.Status, InboxMessageStatus.Retrying)
            .Set(x => x.NextRetryAt, null)
            .Set(x => x.RetryCount, 0);

        var result = await _context.InboxMessages.UpdateOneAsync(
            x => x.Id == inboxMessageId && x.Status == InboxMessageStatus.DeadLetter,
            update,
            cancellationToken: ct);

        if (result.ModifiedCount == 0)
            return new InboxRequeueResult(InboxRequeueOutcome.NotDeadLetter, null, 0);

        var priorRetries = (int)await _context.InboxRetryHistories
            .CountDocumentsAsync(h => h.InboxMessageId == inboxMessageId, cancellationToken: ct);
        var retryNumber = priorRetries + 1;

        var history = InboxRetryHistoryDocument.Start(
            inboxMessageId, doc.MessageId, doc.ConsumerName, doc.Topic, retryNumber, operatorId);
        await _context.InboxRetryHistories.InsertOneAsync(history, cancellationToken: ct);

        var snapshot = ToSnapshot(doc) with { Status = InboxMessageStatus.Retrying, NextRetryAt = null, RetryCount = 0 };
        return new InboxRequeueResult(InboxRequeueOutcome.Requeued, snapshot, retryNumber);
    }

    public async Task RevertFailedRequeueAsync(Guid inboxMessageId, string error, CancellationToken ct = default)
    {
        var update = Builders<InboxDocument>.Update.Set(x => x.Status, InboxMessageStatus.DeadLetter);
        await _context.InboxMessages.UpdateOneAsync(x => x.Id == inboxMessageId, update, cancellationToken: ct);

        await CloseOpenRetryHistoryAsync(inboxMessageId, InboxRetryHistoryResult.FailedAgain, error, ct);
    }

    public async Task<IReadOnlyList<InboxRetryHistoryEntry>> GetRetryHistoryAsync(
        Guid inboxMessageId, CancellationToken ct = default)
    {
        var docs = await _context.InboxRetryHistories
            .Find(h => h.InboxMessageId == inboxMessageId)
            .SortByDescending(h => h.StartedAt)
            .ToListAsync(ct);

        return [.. docs.Select(h => new InboxRetryHistoryEntry(
            h.Id, h.InboxMessageId, h.MessageId, h.ConsumerName, h.Topic, h.RetryNumber,
            h.StartedAt, h.FinishedAt, h.DurationMs, h.Operator, h.Result, h.Exception))];
    }

    private static InboxMessageSnapshot ToSnapshot(InboxDocument m) => new(
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
