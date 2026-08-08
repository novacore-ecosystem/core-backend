namespace NovaCore.Promotion.Application.Features.Coupons.Shared;

/// <summary>
/// The single Coupon-redemption eligibility rule set, shared by ValidateCouponHandler (read-only
/// check) and RedeemCouponHandler (re-evaluated live inside the redemption itself - a prior
/// Validate call's result is never trusted, see Phase 4.4 Section 19). Only checks fields the
/// Coupon Domain already exposes (IsEnabled/Status/StartTime-EndTime/MaxUsage/MaxUsagePerUser) -
/// Coupon.RecordUsage's own doc comment explicitly disclaims eligibility/limit enforcement
/// ("no eligibility/limit enforcement lives here"), so this is where that responsibility
/// genuinely belongs, not a duplication of Domain logic.
/// </summary>
internal static class CouponRedemptionEligibility
{
    public static CouponEligibilityResult Evaluate(Coupon coupon, int userUsageCount)
    {
        if (!coupon.IsEnabled)
            return CouponEligibilityResult.Invalid(MessageCode.CouponDisabled, "Coupon is disabled.");

        if (coupon.Status != CouponStatus.Active)
            return CouponEligibilityResult.Invalid(
                MessageCode.CouponNotActive, $"Coupon is not active (current status: {coupon.Status}).");

        var now = DateTime.UtcNow;
        if (now < coupon.StartTime || now > coupon.EndTime)
            return CouponEligibilityResult.Invalid(
                MessageCode.CouponNotActive, "Coupon is outside its active time window.");

        if (coupon.MaxUsage is { } maxUsage && coupon.CurrentUsage >= maxUsage)
            return CouponEligibilityResult.Invalid(
                MessageCode.CouponUsageLimitReached, "Coupon usage limit has been reached.");

        if (coupon.MaxUsagePerUser is { } maxUsagePerUser && userUsageCount >= maxUsagePerUser)
            return CouponEligibilityResult.Invalid(
                MessageCode.CouponUsageLimitReached, "Coupon usage limit for this user has been reached.");

        return CouponEligibilityResult.Valid;
    }
}

internal sealed record CouponEligibilityResult(bool CanProceed, MessageCode? Code, string? Reason)
{
    public static readonly CouponEligibilityResult Valid = new(true, null, null);

    public static CouponEligibilityResult Invalid(MessageCode code, string reason) => new(false, code, reason);
}
