using NovaCore.Promotion.Application.Abstractions.Persistence;
using NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;
using NovaCore.Promotion.Application.Features.Coupons.Shared;

namespace NovaCore.Promotion.Application.Features.Coupons.Commands.RedeemCoupon;

/// <summary>
/// The whole "reload -> re-check -> mutate" sequence runs inside OptimisticConcurrencyRetry so
/// each retry attempt re-reads authoritative state instead of trusting a stale in-memory copy -
/// required because Coupon.CurrentUsage/MaxUsage enforcement can only be race-safe if a losing
/// concurrent writer (caught as ConflictException by EfUnitOfWork on an xmin mismatch, see
/// CouponConfig's ConfigureCommonFields) re-evaluates eligibility against the winner's committed
/// state before retrying. Same shape as Inventory's DeductStockHandler.
/// </summary>
public sealed class RedeemCouponHandler(
    ICouponReadService couponReadService,
    ICouponWriteService couponWriteService,
    ICurrentUserService currentUser,
    OptimisticConcurrencyRetry concurrencyRetry,
    IUnitOfWork unitOfWork) : ICommandHandler<RedeemCouponCommand, RedeemCouponResponse>
{
    public Task<RedeemCouponResponse> Handle(RedeemCouponCommand request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId() ?? throw new ForbiddenException();

        return concurrencyRetry.ExecuteAsync(
            cancellationToken => ProcessAsync(request, userId, cancellationToken), ct: ct);
    }

    private async Task<RedeemCouponResponse> ProcessAsync(RedeemCouponCommand request, Guid userId, CancellationToken ct)
    {
        var coupon = await couponReadService.GetByCodeAsync(request.Code, ct)
            ?? throw new NotFoundException(nameof(Coupon), request.Code);

        var userUsages = await couponReadService.GetUsagesByCouponAndUserAsync(coupon.Id, userId, ct);

        // Idempotent replay: the same logical redemption (Coupon + User + Order) already
        // succeeded - return the existing record instead of redeeming a second time. Defense in
        // depth alongside the HTTP-level Idempotency-Key middleware (see RedeemCoupon endpoint),
        // which is TTL-bound; this natural-key check is permanent whenever OrderId is supplied.
        if (request.OrderId is { } orderId)
        {
            var existing = userUsages.FirstOrDefault(u => u.OrderId == orderId);
            if (existing is not null)
                return new RedeemCouponResponse(coupon.Id, existing.Id, existing.UsedAt);
        }

        var eligibility = CouponRedemptionEligibility.Evaluate(coupon, userUsages.Count);
        if (!eligibility.CanProceed)
            throw new BusinessRuleException(eligibility.Code!.Value, eligibility.Reason);

        (Guid UsageId, DateTime UsedAt) result = default;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            result = await couponWriteService.RedeemAsync(coupon.Id, userId, request.OrderId, ct);
        }, ct: ct);

        return new RedeemCouponResponse(coupon.Id, result.UsageId, result.UsedAt);
    }
}
