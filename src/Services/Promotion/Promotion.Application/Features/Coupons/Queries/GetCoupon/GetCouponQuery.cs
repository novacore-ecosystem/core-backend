namespace NovaCore.Promotion.Application.Features.Coupons.Queries.GetCoupon;

public sealed record GetCouponQuery(Guid CouponId) : IQuery<GetCouponResponse>;

public sealed record CouponTranslationResponse(string LanguageCode, string Name, string? Description);

public sealed record GetCouponResponse(
    Guid Id,
    Guid PromotionId,
    Guid? CampaignId,
    Guid? BatchId,
    string Code,
    string Name,
    string? Description,
    CouponStatus Status,
    CouponVisibility Visibility,
    CouponType CouponType,
    DateTime StartTime,
    DateTime EndTime,
    string TimeZone,
    int? MaxUsage,
    int? MaxUsagePerUser,
    int CurrentUsage,
    bool IsEnabled,
    IReadOnlyCollection<CouponTranslationResponse> Translations,
    DateTime CreatedAt,
    DateTime UpdatedAt);
