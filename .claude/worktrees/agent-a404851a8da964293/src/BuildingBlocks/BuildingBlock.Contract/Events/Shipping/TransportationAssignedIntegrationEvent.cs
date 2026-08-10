namespace NovaCore.BuildingBlock.Contract.Events.Shipping;

/// <summary>
/// Published once a person and/or vehicle is bound to a Transportation - the point at which the
/// attempt becomes actionable for the carrier and trackable for the customer.
/// </summary>
public sealed record TransportationAssignedIntegrationEvent(
    Guid TenantId,
    Guid TransportationId,
    string TransportationNumber,
    Guid ShipmentId,
    Guid ProviderId,
    Guid? PersonId,
    Guid? VehicleId,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(TransportationAssignedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
