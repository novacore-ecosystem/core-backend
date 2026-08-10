namespace NovaCore.Order.Domain.Enums;

public enum DiscountSource : short
{
    Coupon = 1,
    Campaign = 2,
    ProductPromotion = 3,
    ProductSet = 4,
    MemberRank = 5,
    LoyaltyPoint = 6,
    Manual = 7,
    FlashSale = 8,
    System = 9,
    Unknown = 99,
}
