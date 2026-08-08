namespace NovaCore.Promotion.Application.Features.Promotions.Commands.SubmitPromotion;

public sealed record SubmitPromotionCommand(Guid PromotionId) : ICommand<SubmitPromotionResponse>;

public sealed record SubmitPromotionResponse(Guid ApprovalWorkflowId);
