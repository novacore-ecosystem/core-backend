namespace NovaCore.Promotion.Application.Features.Promotions.Commands.EnablePromotion;

public sealed record EnablePromotionCommand(Guid PromotionId) : ICommand<EnablePromotionResponse>;

public sealed record EnablePromotionResponse;
