namespace NovaCore.Promotion.Domain.Enums;

public enum PromotionConditionOperator : byte
{
    Equals = 0,
    NotEquals = 1,
    GreaterThan = 2,
    GreaterThanOrEqual = 3,
    LessThan = 4,
    LessThanOrEqual = 5,
    Contains = 6,
    In = 7,
    NotIn = 8,
}
