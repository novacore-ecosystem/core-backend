namespace NovaCore.Promotion.Application.Features.Coupons.Commands.CreateCoupon;

public sealed record CreateCouponCommand(
    Guid PromotionId,
    string Code,
    string Name,
    CouponType CouponType,
    DateTime StartTime,
    DateTime EndTime,
    string TimeZone,
    string? Description = null,
    CouponVisibility Visibility = CouponVisibility.Public,
    Guid? CampaignId = null,
    Guid? BatchId = null,
    int? MaxUsage = null,
    int? MaxUsagePerUser = null) : ICommand<CreateCouponResponse>;

public sealed record CreateCouponResponse(Guid CouponId);
