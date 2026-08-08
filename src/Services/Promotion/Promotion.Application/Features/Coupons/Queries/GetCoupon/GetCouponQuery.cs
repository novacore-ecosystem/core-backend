namespace NovaCore.Promotion.Application.Features.Coupons.Queries.GetCoupon;

public sealed record GetCouponQuery(Guid CouponId) : IQuery<GetCouponResponse>;

public sealed record GetCouponResponse(Guid Id, string Code, string Name, CouponStatus Status);
