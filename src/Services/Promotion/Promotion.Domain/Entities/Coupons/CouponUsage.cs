namespace NovaCore.Promotion.Domain.Entities.Coupons;

/// <summary>A single redemption record for a Coupon - no eligibility/limit enforcement lives here, see Coupon.RecordUsage.</summary>
public sealed class CouponUsage : BaseEntity<Guid>, IAuditable
{
    public Guid CouponId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? OrderId { get; private set; }
    public DateTime UsedAt { get; private set; }

    public Coupon Coupon { get; private set; } = default!;

    private CouponUsage() { }

    /// <summary>Only Coupon may construct a CouponUsage - see Coupon.RecordUsage.</summary>
    internal static CouponUsage Create(Guid couponId, Guid userId, Guid? orderId)
    {
        return new CouponUsage
        {
            Id = Guid.CreateVersion7(),
            CouponId = couponId,
            UserId = userId,
            OrderId = orderId,
            UsedAt = DateTime.UtcNow,
        };
    }
}
