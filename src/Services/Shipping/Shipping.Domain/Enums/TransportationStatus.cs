namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Lifecycle of one execution attempt. A terminal Failed/Cancelled attempt does not end the Shipment - a new Transportation can be created for it.</summary>
public enum TransportationStatus
{
    Created = 1,
    Assigned = 2,
    PickedUp = 3,
    InTransit = 4,
    Delivered = 5,
    Failed = 6,
    Returned = 7,
    Cancelled = 8,
}
