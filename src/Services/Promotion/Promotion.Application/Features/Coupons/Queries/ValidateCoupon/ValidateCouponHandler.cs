using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;
using NovaCore.Promotion.Application.Features.Coupons.Shared;

namespace NovaCore.Promotion.Application.Features.Coupons.Queries.ValidateCoupon;

public sealed class ValidateCouponHandler(
    ICouponReadService couponReadService,
    ICurrentUserService currentUser) : IQueryHandler<ValidateCouponQuery, ValidateCouponResponse>
{
    public async Task<ValidateCouponResponse> Handle(ValidateCouponQuery request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId() ?? throw new ForbiddenException();

        var coupon = await couponReadService.GetByCodeAsync(request.Code, ct);
        if (coupon is null)
            return new ValidateCouponResponse(false, "Coupon not found.");

        var userUsageCount = coupon.MaxUsagePerUser is null
            ? 0
            : (await couponReadService.GetUsagesByCouponAndUserAsync(coupon.Id, userId, ct)).Count;

        var eligibility = CouponRedemptionEligibility.Evaluate(coupon, userUsageCount);

        return new ValidateCouponResponse(eligibility.CanProceed, eligibility.Reason);
    }
}
