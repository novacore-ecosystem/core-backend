namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when goods reach the end recipient. This is the event Order Service waits for to
/// move an order to Delivered/Completed.
/// </summary>
public sealed record TransportationDeliveredIntegrationEvent(
    Guid TenantId,
    Guid TransportationId,
    string TransportationNumber,
    Guid ShipmentId,
    Guid ProviderId,
    DateTime DeliveredAt,
    string ReceivedByName,
    bool CodCollected,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(TransportationDeliveredIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
