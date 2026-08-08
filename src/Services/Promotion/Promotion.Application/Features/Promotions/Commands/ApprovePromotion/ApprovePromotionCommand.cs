namespace NovaCore.Promotion.Application.Features.Promotions.Commands.ApprovePromotion;

/// <summary>Approve also Activates the Promotion - PromotionStatus has no separate Scheduled state, so this is the only path from Draft to Active (Promotion.Activate() itself still guards on IsApproved).</summary>
public sealed record ApprovePromotionCommand(Guid PromotionId) : ICommand<ApprovePromotionResponse>;

public sealed record ApprovePromotionResponse;
