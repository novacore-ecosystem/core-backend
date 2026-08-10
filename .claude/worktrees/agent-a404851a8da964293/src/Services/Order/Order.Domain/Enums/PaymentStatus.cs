namespace NovaCore.Order.Domain.Enums;

public enum PaymentStatus : byte
{
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4,
    PartiallyRefunded = 5,
}
