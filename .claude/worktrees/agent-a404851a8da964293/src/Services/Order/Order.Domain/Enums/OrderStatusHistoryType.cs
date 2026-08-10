namespace NovaCore.Order.Domain.Enums;

/// <summary>
/// Classifies an OrderStatusHistory entry. Broader than OrderStatus itself - some values
/// (Paid/Packed/Shipped/Delivered/Returned/Refunded/Reopened) correspond to OrderShipping.Status
/// or future Payment/Return events rather than an Order.Status transition, so PreviousStatus/
/// CurrentStatus on the history row stay nullable and are only populated for entries that really
/// are Order.Status changes.
/// </summary>
public enum OrderStatusHistoryType : byte
{
    Created = 1,
    Updated = 2,
    Confirmed = 3,
    Paid = 4,
    Packed = 5,
    Shipped = 6,
    Delivered = 7,
    Cancelled = 8,
    Returned = 9,
    Refunded = 10,
    Reopened = 11
}
