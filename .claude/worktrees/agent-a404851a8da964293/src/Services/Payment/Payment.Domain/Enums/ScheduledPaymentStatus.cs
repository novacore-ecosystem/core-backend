namespace NovaCore.Payment.Domain.Enums;

public enum ScheduledPaymentStatus : byte
{
    Active = 1,
    Paused = 2,
    Canceled = 3,
    Completed = 4,
}
