namespace NovaCore.Promotion.Application.Features.Coupons.Commands.DisableCoupon;

public sealed record DisableCouponCommand(Guid CouponId) : ICommand<DisableCouponResponse>;

public sealed record DisableCouponResponse;
