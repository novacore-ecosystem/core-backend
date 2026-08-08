namespace NovaCore.Promotion.Application.Features.Coupons.Queries.ValidateCoupon;

/// <summary>
/// Answers "is this Coupon currently valid for redemption?" - never eligibility in the Promotion
/// Engine sense (Order/product/customer-segment applicability). A read/decision operation only -
/// never mutates redemption state (see RedeemCoupon for the state-changing counterpart). UserId is
/// deliberately not a field here - ValidateCouponHandler resolves it from ICurrentUserService,
/// same as every other user-scoped Handler in the platform (e.g. AddCartItemHandler).
/// </summary>
public sealed record ValidateCouponQuery(string Code) : IQuery<ValidateCouponResponse>;

/// <summary>
/// Public-safe validation result - no database IDs, EF state, or internal exceptions exposed.
/// Reason is populated only when CanProceed is false.
/// </summary>
public sealed record ValidateCouponResponse(bool CanProceed, string? Reason);
