namespace NovaCore.Promotion.Application.Features.Promotions.Commands.RejectPromotion;

public sealed record RejectPromotionCommand(Guid PromotionId) : ICommand<RejectPromotionResponse>;

public sealed record RejectPromotionResponse;
