namespace NovaCore.Promotion.Domain.Enums;

public enum PromotionStatus : byte
{
    Draft = 0,
    Active = 1,
    Paused = 2,
    Expired = 3,
    Cancelled = 4,
}
