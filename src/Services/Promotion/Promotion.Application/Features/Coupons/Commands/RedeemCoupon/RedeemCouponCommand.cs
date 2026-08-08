namespace NovaCore.Promotion.Application.Features.Coupons.Commands.RedeemCoupon;

/// <summary>
/// Establishes the Coupon's usage/claim state - never calculates a discount, order total, or
/// stacking outcome (that stays Promotion Engine logic for a later phase). OrderId is optional,
/// matching CouponUsage.OrderId's own nullability - PromotionService never depends on
/// OrderService, it only stores the opaque external identifier the caller supplies. UserId is not
/// a field here - RedeemCouponHandler resolves it from ICurrentUserService, same as
/// ValidateCouponQuery.
/// </summary>
public sealed record RedeemCouponCommand(string Code, Guid? OrderId) : ICommand<RedeemCouponResponse>;

/// <summary>Only what a caller needs - never the full Coupon aggregate or internal persistence metadata.</summary>
public sealed record RedeemCouponResponse(Guid CouponId, Guid UsageId, DateTime UsedAt);
