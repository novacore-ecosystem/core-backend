namespace NovaCore.Payment.Domain.Enums;

public enum PaymentAttemptStatus : byte
{
    Initiated = 1,
    Pending = 2,
    Authorized = 3,
    Captured = 4,
    Failed = 5,
    TimedOut = 6,
    Canceled = 7,
}
