using NovaCore.BuildingBlock.Persistence.Inbox;

using MongoDB.Bson.Serialization.Attributes;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Inbox;

/// <summary>
/// Append-only record of one manually-triggered retry attempt against a dead-lettered
/// InboxDocument. Mongo equivalent of NovaCore.BuildingBlock.Persistence.Ef.Inbox.InboxRetryHistory.
/// </summary>
public sealed class InboxRetryHistoryDocument
{
    [BsonId]
    public Guid Id { get; private set; }
    public Guid InboxMessageId { get; private set; }
    public Guid MessageId { get; private set; }
    public string ConsumerName { get; private set; } = string.Empty;
    public string Topic { get; private set; } = string.Empty;
    public int RetryNumber { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public long? DurationMs { get; private set; }
    public string? Operator { get; private set; }
    public InboxRetryHistoryResult Result { get; private set; }
    public string? Exception { get; private set; }

    private InboxRetryHistoryDocument() { }

    public static InboxRetryHistoryDocument Start(
        Guid inboxMessageId, Guid messageId, string consumerName, string topic, int retryNumber, string? operatorId)
    {
        return new InboxRetryHistoryDocument
        {
            Id = Guid.CreateVersion7(),
            InboxMessageId = inboxMessageId,
            MessageId = messageId,
            ConsumerName = consumerName,
            Topic = topic,
            RetryNumber = retryNumber,
            StartedAt = DateTime.UtcNow,
            Operator = operatorId,
            Result = InboxRetryHistoryResult.Retrying,
        };
    }

    public void Close(InboxRetryHistoryResult result, string? exception)
    {
        FinishedAt = DateTime.UtcNow;
        DurationMs = (long)(FinishedAt.Value - StartedAt).TotalMilliseconds;
        Result = result;
        Exception = exception;
    }
}
