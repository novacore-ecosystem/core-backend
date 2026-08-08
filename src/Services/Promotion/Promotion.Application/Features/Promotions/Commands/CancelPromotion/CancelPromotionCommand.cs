namespace NovaCore.Promotion.Application.Features.Promotions.Commands.CancelPromotion;

/// <summary>"Delete Promotion" (Section 27's API summary) maps to Promotion.Cancel() - no physical delete, same historical-data-retention precedent as Coupon.</summary>
public sealed record CancelPromotionCommand(Guid PromotionId) : ICommand<CancelPromotionResponse>;

public sealed record CancelPromotionResponse;
