namespace NovaCore.Promotion.Application.Features.Coupons.Commands.TranslateCoupon;

/// <summary>
/// One upsert operation - mirrors Coupon.Translate's own upsert behavior (existing translation
/// for the language is updated, otherwise a new one is added). No separate Create/Update
/// translation operations.
/// </summary>
public sealed record TranslateCouponCommand(
    Guid CouponId,
    string LanguageCode,
    string Name,
    string? Description) : ICommand<TranslateCouponResponse>;

public sealed record TranslateCouponResponse;
