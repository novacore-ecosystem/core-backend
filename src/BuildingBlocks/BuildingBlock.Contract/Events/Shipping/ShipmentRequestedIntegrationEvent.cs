namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published when a Shipment leaves Draft and is formally requested - the signal for whoever
/// plans transportation, and for the source module (Order, warehouse, supplier) to learn that
/// its goods are now in the logistics pipeline.
/// </summary>
public sealed record ShipmentRequestedIntegrationEvent(
    Guid TenantId,
    Guid ShipmentId,
    string ShipmentNumber,
    string SourceType,
    Guid SourceReferenceId,
    string ShipmentType,
    int ItemCount,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ShipmentRequestedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
