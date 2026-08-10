namespace NovaCore.Payment.Domain.Enums;

public enum RefundStatus : byte
{
    Requested = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Canceled = 5,
}
