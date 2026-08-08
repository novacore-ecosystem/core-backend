namespace NovaCore.Promotion.Application.Features.Coupons.Commands.CreateCoupon;

public sealed record CreateCouponCommand(string Name) : ICommand<CreateCouponResponse>;

public sealed record CreateCouponResponse(Guid CouponId);
