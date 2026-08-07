namespace NovaCore.Shipping.Domain.Enums;

/// <summary>Outcome of the customer-facing delivery of one Transportation - not every Transportation has one (warehouse transfers do not).</summary>
public enum DeliveryStatus
{
    Pending = 1,
    OutForDelivery = 2,
    Delivered = 3,
    Failed = 4,
    Refused = 5,
    Returned = 6,
}
