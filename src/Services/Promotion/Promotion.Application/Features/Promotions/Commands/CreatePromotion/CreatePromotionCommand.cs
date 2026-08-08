namespace NovaCore.Promotion.Application.Features.Promotions.Commands.CreatePromotion;

/// <summary>
/// Benefits/Targets/Constraints/StackingMode are configured once, at creation - this phase does
/// not expose a way to add/remove them afterward (UpdatePromotion only touches Name/Description/
/// Schedule/Priority, matching UpdateCoupon's own scope boundary). A future phase can add
/// dedicated child-collection endpoints if administration needs to change targeting/discount
/// shape after creation.
/// </summary>
public sealed record CreatePromotionCommand(
    string Code,
    string Name,
    PromotionType Type,
    DateTime StartTime,
    DateTime EndTime,
    string Currency,
    string TimeZone,
    string? Description = null,
    int Priority = 0,
    Guid? CampaignId = null,
    IReadOnlyList<CreatePromotionBenefitRequest>? Benefits = null,
    IReadOnlyList<CreatePromotionTargetRequest>? Targets = null,
    IReadOnlyList<CreatePromotionConstraintRequest>? Constraints = null,
    PromotionStackingMode StackingMode = PromotionStackingMode.NotStackable) : ICommand<CreatePromotionResponse>;

public sealed record CreatePromotionBenefitRequest(PromotionBenefitType BenefitType, decimal Value);

public sealed record CreatePromotionTargetRequest(PromotionTargetType TargetType, string TargetKey);

public sealed record CreatePromotionConstraintRequest(PromotionConstraintType ConstraintType, string Value);

public sealed record CreatePromotionResponse(Guid PromotionId);
