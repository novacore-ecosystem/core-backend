namespace NovaCore.Promotion.Application.Features.Coupons.Commands.UpdateCoupon;

public sealed record UpdateCouponCommand(
    Guid CouponId,
    string Name,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    string TimeZone,
    CouponVisibility Visibility,
    int? MaxUsage,
    int? MaxUsagePerUser) : ICommand<UpdateCouponResponse>;

public sealed record UpdateCouponResponse;
