using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

namespace NovaCore.Promotion.Application.Features.Coupons.Commands.TranslateCoupon;

public sealed class TranslateCouponHandler(
    ICouponReadService couponReadService,
    ICouponWriteService couponWriteService) : ICommandHandler<TranslateCouponCommand, TranslateCouponResponse>
{
    public async Task<TranslateCouponResponse> Handle(TranslateCouponCommand request, CancellationToken ct = default)
    {
        _ = await couponReadService.GetByIdAsync(request.CouponId, ct)
            ?? throw new NotFoundException(nameof(Coupon), request.CouponId);

        await couponWriteService.TranslateAsync(
            request.CouponId, request.LanguageCode, request.Name, request.Description, ct);

        return new TranslateCouponResponse();
    }
}
