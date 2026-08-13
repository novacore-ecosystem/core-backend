namespace NovaCore.BuildingBlock.Contract.Events.Tenant;

/// <summary>
/// Published whenever a bootstrap-affecting Tenant change bumps Tenant.Version (see
/// docs/services/auth-service.md, "Versioning"). Backend foundation for the future Notification
/// Hub version-check flow: a consumer refreshes the Redis version cache and notifies connected
/// clients in the tenant's SignalR group - see Notification.Infrastructure's
/// TenantVersionChangedIntegrationEventConsumer. Carries only the new version, not the changed
/// content itself - consumers that need the content re-fetch it (bootstrap/detail), same
/// principle as every other integration event in this solution.
/// </summary>
public sealed record TenantVersionChangedIntegrationEvent(
    Guid TenantId,
    int Version,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(TenantVersionChangedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
