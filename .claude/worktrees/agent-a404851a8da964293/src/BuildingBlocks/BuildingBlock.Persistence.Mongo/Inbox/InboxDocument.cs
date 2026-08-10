using NovaCore.BuildingBlock.Persistence.Inbox;

using MongoDB.Bson.Serialization.Attributes;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Inbox;

/// <summary>
/// Dedup + retry marker: one document per (MessageId, ConsumerName) pair. Keyed per-consumer so
/// redelivered messages don't replay across multiple consumers listening to the same topic.
/// Topic/Payload/Headers are captured so a failed attempt can be replayed later by
/// InboxRetryHostedService without needing the original Kafka message to still be available.
/// </summary>
public sealed class InboxDocument
{
    [BsonId]
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

    private InboxDocument() { }

    public static InboxDocument Create(Guid messageId, string consumerName, string topic, string payload, string headersJson)
    {
        return new InboxDocument
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

        if (double.IsInfinity(delayMs) || delayMs > policy.MaximumRetryDelay.TotalMilliseconds)
            return policy.MaximumRetryDelay;

        return TimeSpan.FromMilliseconds(delayMs);
    }
}
