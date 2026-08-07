namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when goods have physically been picked up and the attempt is under way.
/// </summary>
public sealed record TransportationStartedIntegrationEvent(
    Guid TenantId,
    Guid TransportationId,
    string TransportationNumber,
    Guid ShipmentId,
    Guid ProviderId,
    DateTime StartedAt,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(TransportationStartedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
