namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Lifecycle of the shipment *intention*. Execution progress lives on Transportation - a Shipment stays InTransit across several failed Transportation attempts.</summary>
public enum ShipmentStatus
{
    Draft = 1,
    Requested = 2,
    Planned = 3,
    InTransit = 4,
    Delivered = 5,
    Failed = 6,
    Returned = 7,
    Cancelled = 8,
}
