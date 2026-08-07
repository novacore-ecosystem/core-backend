namespace NovaCore.Promotion.Domain.Enums;

// TODO (structural placeholder): exact taxonomy not yet specified by the architect's design -
// values below are a conventional promotion-engine set, confirm/replace when the design is available.
public enum PromotionType : byte
{
    PercentageOff = 0,
    FixedAmountOff = 1,
    BuyXGetY = 2,
    FreeShipping = 3,
    Bundle = 4,
    Custom = 99,
}
