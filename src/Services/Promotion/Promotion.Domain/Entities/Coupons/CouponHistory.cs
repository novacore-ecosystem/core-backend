namespace NovaCore.Promotion.Domain.Entities.Coupons;

/// <summary>An append-only audit trail row for a Coupon - CreatedAt is inherited from BaseEntity, not redeclared.</summary>
public sealed class CouponHistory : BaseEntity<Guid>, IAuditable
{
    public Guid CouponId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    public Coupon Coupon { get; private set; } = default!;

    private CouponHistory() { }

    /// <summary>Only Coupon may construct a CouponHistory - see Coupon.RecordHistory.</summary>
    internal static CouponHistory Create(Guid couponId, string action, Guid? operatorId)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("History action cannot be empty.");

        return new CouponHistory
        {
            Id = Guid.CreateVersion7(),
            CouponId = couponId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
