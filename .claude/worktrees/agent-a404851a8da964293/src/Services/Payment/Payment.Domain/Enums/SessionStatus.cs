namespace NovaCore.Payment.Domain.Enums;

public enum SessionStatus : byte
{
    Open = 1,
    Completed = 2,
    Expired = 3,
    Canceled = 4,
}
