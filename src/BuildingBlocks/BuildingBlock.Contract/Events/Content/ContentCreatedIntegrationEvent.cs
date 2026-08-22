namespace NovaCore.BuildingBlock.Contract.Events.Content;

/// <summary>Published when a new Content item is created. Consumed by search/index sync and WCM read-models.</summary>
public sealed record ContentCreatedIntegrationEvent(
    Guid ContentId,
    Guid ContentTypeId,
    string Slug,
    Guid CreatedBy,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ContentCreatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
