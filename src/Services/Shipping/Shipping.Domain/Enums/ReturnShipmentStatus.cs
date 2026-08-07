namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Lifecycle of a return request raised against an already-shipped Shipment.</summary>
public enum ReturnShipmentStatus
{
    Requested = 1,
    Approved = 2,
    InTransit = 3,
    Completed = 4,
    Rejected = 5,
    Cancelled = 6,
}
