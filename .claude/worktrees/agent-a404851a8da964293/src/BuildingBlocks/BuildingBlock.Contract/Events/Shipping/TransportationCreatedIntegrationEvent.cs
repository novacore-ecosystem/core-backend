namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when a new execution attempt is created against a Shipment. AttemptNo > 1 means
/// this is a retry after a previous attempt failed - the Shipment itself was never recreated.
/// </summary>
public sealed record TransportationCreatedIntegrationEvent(
    Guid TenantId,
    Guid TransportationId,
    string TransportationNumber,
    Guid ShipmentId,
    Guid ProviderId,
    int AttemptNo,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(TransportationCreatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
