namespace NovaCore.BuildingBlock.Contract.Events.Audit;

/// <summary>
/// The single audit event type in the project. Published once per changed Aggregate Root
/// instance per SaveChanges - never once per entity - by the change-tracking pipeline. Carries
/// the full aggregate's change graph (<see cref="Root"/>), so a consumer never needs a second
/// event to reconstruct "what else changed alongside this". <see cref="RootEntityType"/>/
/// <see cref="RootEntityId"/> duplicate <c>Root.EntityType</c>/<c>Root.EntityId</c> deliberately -
/// they exist as flat, top-level fields so a consumer (e.g. a MongoDB-backed Audit Service) can
/// index/query by aggregate identity without descending into the nested document.
/// </summary>
public sealed record AuditIntegrationEvent(
    string RootEntityType,
    string RootEntityId,
    AuditNode Root,
    AuditMetadata? Metadata = null,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(AuditIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
