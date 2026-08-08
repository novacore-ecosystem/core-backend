using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

namespace NovaCore.Promotion.Application.Features.Coupons.Commands.CreateCoupon;

public sealed class CreateCouponHandler(ICouponWriteService couponWriteService)
    : ICommandHandler<CreateCouponCommand, CreateCouponResponse>
{
    public async Task<CreateCouponResponse> Handle(CreateCouponCommand request, CancellationToken ct = default)
    {
        var coupon = Coupon.Create(
            request.PromotionId,
            EntityCode.Create(request.Code),
            request.Name,
            request.CouponType,
            request.StartTime,
            request.EndTime,
            request.TimeZone,
            request.Description,
            request.Visibility,
            request.CampaignId,
            request.BatchId,
            request.MaxUsage,
            request.MaxUsagePerUser);

        await couponWriteService.CreateAsync(coupon, ct);

        return new CreateCouponResponse(coupon.Id);
    }
}
