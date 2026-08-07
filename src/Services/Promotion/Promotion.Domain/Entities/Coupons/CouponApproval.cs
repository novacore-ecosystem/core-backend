namespace NovaCore.Promotion.Domain.Entities.Coupons;

/// <summary>
/// Structural record of a Coupon's approval review. No real approval workflow yet - same deferral
/// as CampaignApproval, owned by a later Phase 2.x (Approval + Validation + Audit).
/// </summary>
public sealed class CouponApproval : BaseEntity<Guid>, IAuditable
{
    public Guid CouponId { get; private set; }
    public string? Status { get; private set; }
    public Guid? ReviewerId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? Comment { get; private set; }

    public Coupon Coupon { get; private set; } = default!;

    private CouponApproval() { }

    public static CouponApproval Create(Guid couponId)
    {
        return new CouponApproval
        {
            Id = Guid.CreateVersion7(),
            CouponId = couponId,
        };
    }

    // TODO (later Phase 2.x): replace these structural stand-ins with the real approval workflow
    // (status enum, validation rules, audit trail) once that phase's design is available.
    public void RecordReview(Guid reviewerId, string status, string? comment)
    {
        ReviewerId = reviewerId;
        Status = status;
        Comment = comment;
        ApprovedAt = DateTime.UtcNow;
    }
}
