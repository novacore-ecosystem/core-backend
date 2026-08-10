namespace NovaCore.Order.Domain.Enums;

public enum ReturnStatus : byte
{
    Requested = 1,
    Approved = 2,
    Rejected = 3,
    ShippingBack = 4,
    Received = 5,
    Inspecting = 6,
    Refunding = 7,
    Completed = 8,
    Cancelled = 9,
}
