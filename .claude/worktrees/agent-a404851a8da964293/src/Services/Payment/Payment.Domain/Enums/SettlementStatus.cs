namespace NovaCore.Payment.Domain.Enums;

public enum SettlementStatus : byte
{
    Pending = 1,
    Settled = 2,
    Failed = 3,
}
