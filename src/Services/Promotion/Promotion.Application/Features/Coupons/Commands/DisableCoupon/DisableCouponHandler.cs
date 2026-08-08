using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

namespace NovaCore.Promotion.Application.Features.Coupons.Commands.DisableCoupon;

/// <summary>
/// Disables (IsEnabled = false), never physically deletes - Coupon has no Delete method, and
/// its Domain lifecycle is designed to keep historical Coupon data in place (see Coupon.Disable).
/// </summary>
public sealed class DisableCouponHandler(
    ICouponReadService couponReadService,
    ICouponWriteService couponWriteService) : ICommandHandler<DisableCouponCommand, DisableCouponResponse>
{
    public async Task<DisableCouponResponse> Handle(DisableCouponCommand request, CancellationToken ct = default)
    {
        _ = await couponReadService.GetByIdAsync(request.CouponId, ct)
            ?? throw new NotFoundException(nameof(Coupon), request.CouponId);

        await couponWriteService.DisableAsync(request.CouponId, ct);

        return new DisableCouponResponse();
    }
}
