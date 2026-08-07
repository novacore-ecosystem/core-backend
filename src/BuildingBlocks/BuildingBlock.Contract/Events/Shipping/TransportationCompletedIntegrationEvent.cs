namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when an attempt finishes successfully. Distinct from TransportationDelivered: this
/// fires for every kind of transportation (including a warehouse transfer), whereas Delivered is
/// specifically about handing goods to an end recipient.
/// </summary>
public sealed record TransportationCompletedIntegrationEvent(
    Guid TenantId,
    Guid TransportationId,
    string TransportationNumber,
    Guid ShipmentId,
    Guid ProviderId,
    DateTime CompletedAt,
    decimal TotalCost,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(TransportationCompletedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
