namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when an attempt fails. Deliberately not terminal for the Shipment - a consumer may
/// respond by creating another Transportation rather than failing the whole shipment.
/// </summary>
public sealed record TransportationFailedIntegrationEvent(
    Guid TenantId,
    Guid TransportationId,
    string TransportationNumber,
    Guid ShipmentId,
    Guid ProviderId,
    int AttemptNo,
    string Reason,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(TransportationFailedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
