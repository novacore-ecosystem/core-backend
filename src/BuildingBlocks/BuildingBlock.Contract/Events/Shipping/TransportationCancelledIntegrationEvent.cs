namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when an in-flight attempt is called off (provider unavailable, shipment cancelled
/// upstream, re-planning to a different provider).
/// </summary>
public sealed record TransportationCancelledIntegrationEvent(
    Guid TenantId,
    Guid TransportationId,
    string TransportationNumber,
    Guid ShipmentId,
    Guid ProviderId,
    string Reason,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(TransportationCancelledIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
