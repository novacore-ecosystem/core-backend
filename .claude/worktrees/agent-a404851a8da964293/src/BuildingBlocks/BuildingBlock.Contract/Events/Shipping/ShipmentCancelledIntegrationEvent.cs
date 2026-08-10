namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when a Shipment is cancelled before completion. The source module uses this to
/// release whatever it reserved (stock, an order's fulfilment hold) - ShippingService itself
/// never reaches into another module to undo anything.
/// </summary>
public sealed record ShipmentCancelledIntegrationEvent(
    Guid TenantId,
    Guid ShipmentId,
    string ShipmentNumber,
    string SourceType,
    Guid SourceReferenceId,
    string Reason,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ShipmentCancelledIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
