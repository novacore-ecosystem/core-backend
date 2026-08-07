namespace NovaCore.Promotion.Domain.Entities.Coupons;

/// <summary>An append-only version snapshot marker for a Coupon - no real snapshot payload semantics enforced here, mirrors PromotionVersion.</summary>
public sealed class CouponVersion : BaseEntity<Guid>, IAuditable
{
    public Guid CouponId { get; private set; }
    public int Version { get; private set; }
    public string? Snapshot { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    public Coupon Coupon { get; private set; } = default!;

    private CouponVersion() { }

    /// <summary>Only Coupon may construct a CouponVersion - see Coupon.AddVersion.</summary>
    internal static CouponVersion Create(Guid couponId, int version, string? snapshot)
    {
        return new CouponVersion
        {
            Id = Guid.CreateVersion7(),
            CouponId = couponId,
            Version = version,
            Snapshot = snapshot,
        };
    }

    public void Publish()
    {
        PublishedAt = DateTime.UtcNow;
    }
}
