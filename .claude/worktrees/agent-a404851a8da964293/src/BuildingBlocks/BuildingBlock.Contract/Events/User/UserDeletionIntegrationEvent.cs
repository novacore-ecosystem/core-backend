namespace NovaCore.BuildingBlock.Contract.Events.User;

public sealed record UserDeletionIntegrationEvent(
    string UserId,
    string Reason,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; } = nameof(UserDeletionIntegrationEvent);
    public DateTime PublishedAt { get; } = DateTime.UtcNow;
}
