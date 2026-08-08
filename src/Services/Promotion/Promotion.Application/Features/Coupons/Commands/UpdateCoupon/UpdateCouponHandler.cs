using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

namespace NovaCore.Promotion.Application.Features.Coupons.Commands.UpdateCoupon;

public sealed class UpdateCouponHandler(
    ICouponReadService couponReadService,
    ICouponWriteService couponWriteService,
    IUnitOfWork uow) : ICommandHandler<UpdateCouponCommand, UpdateCouponResponse>
{
    public async Task<UpdateCouponResponse> Handle(UpdateCouponCommand request, CancellationToken ct = default)
    {
        _ = await couponReadService.GetByIdAsync(request.CouponId, ct)
            ?? throw new NotFoundException(nameof(Coupon), request.CouponId);

        await uow.ExecuteTransactionAsync(async () =>
        {
            await couponWriteService.UpdateDetailsAsync(
                request.CouponId,
                request.Name,
                request.Description,
                request.StartTime,
                request.EndTime,
                request.TimeZone,
                request.Visibility,
                request.MaxUsage,
                request.MaxUsagePerUser,
                ct);
        }, ct: ct);

        return new UpdateCouponResponse();
    }
}
