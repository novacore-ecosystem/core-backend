namespace NovaCore.Promotion.Domain.Enums;

// TODO (structural placeholder): exact taxonomy not yet specified by the architect's design -
// values below are a conventional promotion-engine set, confirm/replace when the design is available.
public enum PromotionTargetType : byte
{
    Product = 0,
    Category = 1,
    Sku = 2,
    Cart = 3,
    Customer = 4,
    Order = 5,
}
