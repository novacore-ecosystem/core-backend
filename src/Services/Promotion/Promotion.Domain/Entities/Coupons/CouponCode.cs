namespace NovaCore.Promotion.Domain.Entities.Coupons;

/// <summary>
/// An individually issued, redeemable code under a Coupon (e.g. one of many unique codes
/// generated in a CouponBatch import) - distinct from Coupon.Code (the coupon's own/template
/// code, though both now share the EntityCode Value Object after the Phase 2.5 consolidation).
/// Construction stays public, not internal - no Coupon aggregate method creates it directly.
/// </summary>
public sealed class CouponCode : BaseEntity<Guid>, IAuditable
{
    public Guid CouponId { get; private set; }
    public Guid? BatchId { get; private set; }
    public EntityCode Code { get; private set; } = default!;
    public bool IsUsed { get; private set; }

    public Coupon Coupon { get; private set; } = default!;
    public CouponBatch? Batch { get; private set; }

    private CouponCode() { }

    public static CouponCode Create(Guid couponId, EntityCode code, Guid? batchId = null)
    {
        return new CouponCode
        {
            Id = Guid.CreateVersion7(),
            CouponId = couponId,
            BatchId = batchId,
            Code = code,
            IsUsed = false,
        };
    }

    /// <summary>Structural flag flip only - no redemption/eligibility logic lives here.</summary>
    public void MarkUsed()
    {
        IsUsed = true;
    }
}
