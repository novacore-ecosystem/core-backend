namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Lifecycle of a scheduled collection. Separate from ShipmentStatus - a Pickup can fail and be rescheduled without the Shipment changing state.</summary>
public enum PickupStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
}
