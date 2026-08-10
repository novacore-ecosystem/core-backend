namespace NovaCore.Payment.Domain.Enums;

public enum PayoutStatus : byte
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
}
