namespace NovaCore.Payment.Domain.Enums;

public enum RefundAttemptStatus : byte
{
    Initiated = 1,
    Pending = 2,
    Succeeded = 3,
    Failed = 4,
    TimedOut = 5,
    Canceled = 6,
}
