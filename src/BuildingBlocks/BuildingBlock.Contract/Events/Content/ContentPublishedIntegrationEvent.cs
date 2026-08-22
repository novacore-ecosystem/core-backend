namespace NovaCore.BuildingBlock.Contract.Events.Content;

/// <summary>Published when a Content's version goes live. Consumed by search/index sync, WCM cache invalidation, and any downstream product surfacing published content.</summary>
public sealed record ContentPublishedIntegrationEvent(
    Guid ContentId,
    Guid VersionId,
    string Slug,
    DateTime ContentPublishedAt,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ContentPublishedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
