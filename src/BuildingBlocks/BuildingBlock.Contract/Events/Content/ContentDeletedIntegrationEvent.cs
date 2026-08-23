namespace NovaCore.BuildingBlock.Contract.Events.Content;

/// <summary>Published when a Content item is soft-deleted. Consumed by search/index sync to drop
/// the item from the index without waiting for the hard-delete retention job.</summary>
public sealed record ContentDeletedIntegrationEvent(
    Guid ContentId,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ContentDeletedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
