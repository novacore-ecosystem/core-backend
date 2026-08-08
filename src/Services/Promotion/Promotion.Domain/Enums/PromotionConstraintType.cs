namespace NovaCore.Promotion.Domain.Enums;

// TODO (structural placeholder): exact taxonomy not yet specified by the architect's design -
// values below are a conventional promotion-engine set, confirm/replace when the design is available.
public enum PromotionConstraintType : byte
{
    MinimumOrderAmount = 0,
    MinimumQuantity = 1,
    CustomerSegment = 2,
    ProductCategory = 3,
    PaymentMethod = 4,
    MaximumOrderAmount = 5,
    MaximumQuantity = 6,
    MaximumDiscountAmount = 7,
}
