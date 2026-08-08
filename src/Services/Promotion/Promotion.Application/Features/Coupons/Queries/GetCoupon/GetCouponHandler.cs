using Mapster;

using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

namespace NovaCore.Promotion.Application.Features.Coupons.Queries.GetCoupon;

public sealed class GetCouponHandler(ICouponReadService couponReadService) : IQueryHandler<GetCouponQuery, GetCouponResponse>
{
    public async Task<GetCouponResponse> Handle(GetCouponQuery request, CancellationToken ct = default)
    {
        var coupon = await couponReadService.GetByIdAsync(request.CouponId, ct)
            ?? throw new NotFoundException(nameof(Coupon), request.CouponId);

        return coupon.Adapt<GetCouponResponse>();
    }
}
