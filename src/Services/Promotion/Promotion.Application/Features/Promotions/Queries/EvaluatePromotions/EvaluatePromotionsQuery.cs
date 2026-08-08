namespace NovaCore.Promotion.Application.Features.Promotions.Queries.EvaluatePromotions;

/// <summary>
/// PromotionService calculates Promotion effects only - it never returns a final Order total
/// (OrderService remains responsible for that). Deliberately not the full Order entity - only the
/// fields the current evaluation logic (targeting/constraints/discount) actually needs. Coupon-
/// and Campaign-triggered evaluation are out of scope this phase (Section 2) - only automatically
/// triggered Promotions (PromotionExecutionMode.Automatic) are considered.
/// </summary>
public sealed record EvaluatePromotionsQuery(
    Guid? UserId,
    string Currency,
    decimal OrderAmount,
    IReadOnlyList<EvaluationItemRequest> Items) : IQuery<EvaluatePromotionsResponse>;

public sealed record EvaluationItemRequest(
    Guid? ProductId,
    Guid? VariantId,
    Guid? CategoryId,
    int Quantity,
    decimal UnitPrice);

public sealed record EvaluatePromotionsResponse(
    bool IsEligible,
    decimal TotalDiscountAmount,
    IReadOnlyList<AppliedPromotionResult> AppliedPromotions);

/// <summary>
/// AffectedProductIds/AffectedVariantIds are empty when the Promotion targets the entire order
/// (Cart/Order target type, or no targets configured). Gift Items are omitted - the current
/// Promotion Domain has no structural Gift-detail entity to populate them from (Section 13's own
/// "if the Domain already contains Product Gift entities" condition is false here).
/// </summary>
public sealed record AppliedPromotionResult(
    Guid PromotionId,
    string PromotionCode,
    string PromotionName,
    int Priority,
    int ApplyOrder,
    PromotionType Type,
    PromotionBenefitType BenefitType,
    decimal DiscountAmount,
    IReadOnlyList<Guid> AffectedProductIds,
    IReadOnlyList<Guid> AffectedVariantIds,
    string Reason);
