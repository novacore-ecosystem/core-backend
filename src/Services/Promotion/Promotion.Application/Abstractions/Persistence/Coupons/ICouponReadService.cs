namespace NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

public interface ICouponReadService
{
    Task<Coupon?> GetByIdAsync(Guid couponId, CancellationToken ct = default);

    /// <summary>Used by Validate/Redeem - Code is the identifier a checkout-flow caller has, not the internal CouponId.</summary>
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<(IReadOnlyList<Coupon> Items, int TotalCount)> SearchAsync(
        CouponStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Backs both the MaxUsagePerUser eligibility check and Redeem's Coupon+User+Order idempotency check.</summary>
    Task<IReadOnlyList<CouponUsage>> GetUsagesByCouponAndUserAsync(
        Guid couponId,
        Guid userId,
        CancellationToken ct = default);
}
