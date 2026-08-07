namespace NovaCore.Promotion.Domain.Enums;

// TODO (structural placeholder): exact taxonomy not yet specified by the architect's design -
// values below are a conventional promotion-engine set, confirm/replace when the design is available.
public enum PromotionBenefitType : byte
{
    PercentageOff = 0,
    FixedAmountOff = 1,
    FreeShipping = 2,
    FreeGift = 3,
}
