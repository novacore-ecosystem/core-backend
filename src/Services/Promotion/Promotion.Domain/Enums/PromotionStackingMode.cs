namespace NovaCore.Promotion.Domain.Enums;

public enum PromotionStackingMode : byte
{
    NotStackable = 0,
    StackWithAny = 1,
    StackWithSameType = 2,
    StackWithSpecific = 3,
}
