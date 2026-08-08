namespace NovaCore.Promotion.Application.Abstractions.Persistence.Coupons;

public interface ICouponReadService
{
    Task<Coupon?> GetByIdAsync(Guid couponId, CancellationToken ct = default);

    Task<(IReadOnlyList<Coupon> Items, int TotalCount)> SearchAsync(
        CouponStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
