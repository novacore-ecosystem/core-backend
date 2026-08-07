namespace NovaCore.Promotion.Domain.Enums;

public enum PointTransactionType : byte
{
    Earn = 0,
    Spend = 1,
    Refund = 2,
    Expire = 3,
    Adjust = 4,
    Reward = 5,
    Promotion = 6,
}
