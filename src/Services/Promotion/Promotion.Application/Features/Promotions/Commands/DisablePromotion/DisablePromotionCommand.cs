namespace NovaCore.Promotion.Application.Features.Promotions.Commands.DisablePromotion;

public sealed record DisablePromotionCommand(Guid PromotionId) : ICommand<DisablePromotionResponse>;

public sealed record DisablePromotionResponse;
