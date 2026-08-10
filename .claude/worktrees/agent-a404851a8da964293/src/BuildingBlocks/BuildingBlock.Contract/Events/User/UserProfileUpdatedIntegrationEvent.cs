namespace NovaCore.BuildingBlock.Contract.Events.User;

/// <summary>
/// Fired whenever UpdateUserHandler changes a UserProfile. Today's only consumer is User's own
/// Search sync (self-consumption, see docs/reference/search.md) - previously UpdateUserHandler
/// published nothing at all, which would have left the search index silently stale on every edit.
/// </summary>
public sealed record UserProfileUpdatedIntegrationEvent(
    Guid UserId, string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(UserProfileUpdatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
