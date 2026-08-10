using MongoDB.Bson.Serialization.Attributes;

namespace NovaCore.BuildingBlock.Persistence.Mongo.Outbox;

public sealed class OutboxDocument
{
    [BsonId]
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Topic { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string? ActorId { get; private set; }
    public string ActorType { get; private set; } = "system";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    private OutboxDocument() { }

    public static OutboxDocument Create(
        string eventType, string topic, string payload, string correlationId, string? actorId, string actorType)
    {
        return new OutboxDocument
        {
            Id = Guid.CreateVersion7(),
            EventType = eventType,
            Topic = topic,
            Payload = payload,
            CorrelationId = correlationId,
            ActorId = actorId,
            ActorType = actorType,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void MarkProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
    }
}
