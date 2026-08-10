using NovaCore.BuildingBlock.Persistence.Inbox;

namespace NovaCore.BuildingBlock.Persistence.Ef.Inbox;

/// <summary>
/// Dedup + retry marker: one row per (MessageId, ConsumerName) pair. Keyed per-consumer so
/// redelivered messages don't replay across multiple consumers listening to the same topic.
/// Topic/Payload/Headers are captured so a failed attempt can be replayed later by
/// InboxRetryHostedService without needing the original Kafka message to still be available.
/// </summary>
public sealed class InboxMessage
{
    public Guid Id { get; private set; }
    public Guid MessageId { get; private set; }
    public string ConsumerName { get; private set; } = string.Empty;
    public string Topic { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string HeadersJson { get; private set; } = string.Empty;
    public InboxMessageStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? NextRetryAt { get; private set; }
    public DateTime? LastRetryAt { get; private set; }
    public string? LastError { get; private set; }

    private InboxMessage() { }

    public static InboxMessage Create(
        Guid messageId,
        string consumerName,
        string topic,
        string payload,
        string headersJson)
    {
        return new InboxMessage
        {
            Id = Guid.CreateVersion7(),
            MessageId = messageId,
            ConsumerName = consumerName,
            Topic = topic,
            Payload = payload,
            HeadersJson = headersJson,
            Status = InboxMessageStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Re-captures Topic/Payload/Headers on a pre-existing Retrying row. Harmless no-op when the
    /// values are unchanged; guards against a stale payload if the event contract ever changes.
    /// </summary>
    public void RefreshAttemptContext(
        string topic,
        string payload,
        string headersJson)
    {
        Topic = topic;
        Payload = payload;
        HeadersJson = headersJson;
    }

    /// <summary>
    /// Optimistically stages this row as Processed. Called BEFORE the handler runs so that
    /// whichever SaveChanges the handler's own unit of work performs commits this marker
    /// atomically together with the business change - see EfInboxStore.BeginAttemptAsync.
    /// </summary>
    public void MarkProcessed()
    {
        Status = InboxMessageStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        NextRetryAt = null;
        LastError = null;
    }

    public void MarkFailed(string error, InboxRetryPolicy policy)
    {
        RetryCount++;
        LastError = error;
        LastRetryAt = DateTime.UtcNow;
        ProcessedAt = null;

        if (RetryCount >= policy.MaxRetryCount)
        {
            Status = InboxMessageStatus.DeadLetter;
            NextRetryAt = null;
        }
        else
        {
            Status = InboxMessageStatus.Retrying;
            NextRetryAt = DateTime.UtcNow + ComputeRetryDelay(policy);
        }
    }

    private TimeSpan ComputeRetryDelay(InboxRetryPolicy policy)
    {
        var exponent = Math.Max(0, RetryCount - 1);
        var multiplier = Math.Pow(policy.RetryBackoffMultiplier, exponent);
        var delayMs = policy.InitialRetryDelay.TotalMilliseconds * multiplier;

        if (double.IsInfinity(delayMs)
            || delayMs > policy.MaximumRetryDelay.TotalMilliseconds)
            return policy.MaximumRetryDelay;

        return TimeSpan.FromMilliseconds(delayMs);
    }
}
