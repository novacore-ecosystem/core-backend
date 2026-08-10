namespace NovaCore.Payment.Domain.Enums;

public enum PaymentStatus : byte
{
    Pending = 1,
    Authorized = 2,
    Captured = 3,
    PartiallyRefunded = 4,
    Refunded = 5,
    Failed = 6,
    Canceled = 7,
    Expired = 8,
}
