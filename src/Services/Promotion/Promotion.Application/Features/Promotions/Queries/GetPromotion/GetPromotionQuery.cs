namespace NovaCore.Promotion.Application.Features.Promotions.Queries.GetPromotion;

public sealed record GetPromotionQuery(Guid PromotionId) : IQuery<GetPromotionResponse>;

public sealed record GetPromotionResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    PromotionStatus Status,
    PromotionType Type,
    int Priority,
    int Version,
    DateTime StartTime,
    DateTime EndTime,
    string Currency,
    string TimeZone,
    bool IsEnabled,
    bool IsApproved,
    Guid? ApprovalWorkflowId,
    Guid? CampaignId,
    IReadOnlyList<PromotionBenefitResponse> Benefits,
    IReadOnlyList<PromotionTargetResponse> Targets,
    IReadOnlyList<PromotionConstraintResponse> Constraints,
    PromotionStackingMode? StackingMode);

public sealed record PromotionBenefitResponse(Guid Id, PromotionBenefitType BenefitType, decimal Value);

public sealed record PromotionTargetResponse(Guid Id, PromotionTargetType TargetType, string TargetKey);

public sealed record PromotionConstraintResponse(Guid Id, PromotionConstraintType ConstraintType, string Value);
