namespace NovaCore.Promotion.Application.Features.Promotions.Commands.UpdatePromotion;

/// <summary>Deliberately scoped to Name/Description/Schedule/Priority only - same boundary UpdateCoupon draws (no Status transition, no Target/Benefit/Constraint replacement here).</summary>
public sealed record UpdatePromotionCommand(
    Guid PromotionId,
    string Name,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    string TimeZone,
    int Priority) : ICommand<UpdatePromotionResponse>;

public sealed record UpdatePromotionResponse;
