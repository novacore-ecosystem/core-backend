namespace NovaCore.Shipping.Domain.Entities.Shipments;

/// <summary>
/// Append-only timeline entry for a Shipment - one row per status transition. Immutable by
/// design (no mutating methods): this is the audit trail of the intention's own lifecycle, and is
/// separate from TransportationTracking, which records the physical progress of one execution
/// attempt.
/// </summary>
public sealed class ShipmentEvent : BaseEntity<long>, IAuditable
{
    public Guid ShipmentId { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? ActorId { get; private set; }

    private ShipmentEvent() { }

    internal static ShipmentEvent Record(
        Guid shipmentId,
        ShipmentStatus status,
        string description,
        Guid? actorId = null)
    {
        return new ShipmentEvent
        {
            ShipmentId = shipmentId,
            Status = status,
            OccurredAt = DateTime.UtcNow,
            Description = string.IsNullOrWhiteSpace(description) ? status.ToString() : description.Trim(),
            ActorId = actorId,
        };
    }
}
