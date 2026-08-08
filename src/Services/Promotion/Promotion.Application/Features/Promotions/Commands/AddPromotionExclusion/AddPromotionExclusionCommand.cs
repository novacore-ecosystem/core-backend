namespace NovaCore.Promotion.Application.Features.Promotions.Commands.AddPromotionExclusion;

/// <summary>Records that PromotionId cannot stack with ExcludedPromotionId - checked by EvaluatePromotions' stacking pass regardless of StackingMode (Section 19/21).</summary>
public sealed record AddPromotionExclusionCommand(Guid PromotionId, Guid ExcludedPromotionId) : ICommand<AddPromotionExclusionResponse>;

public sealed record AddPromotionExclusionResponse;
