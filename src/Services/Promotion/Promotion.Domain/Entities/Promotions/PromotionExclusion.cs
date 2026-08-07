namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>
/// Explicit pairwise exclusion between two Promotions - both are independent aggregate roots, so
/// this row (not a raw id collection) is how one Promotion references another it cannot stack
/// with. Same pattern as Order.Domain.Entities.Orders.OrderTag - no navigation properties, only
/// ids; callers that need details join in the query layer. No own Id (pure mapping, Rule 4).
/// </summary>
public sealed class PromotionExclusion : BaseEntity
{
    public Guid PromotionId { get; private set; }
    public Guid ExcludedPromotionId { get; private set; }

    private PromotionExclusion() { }

    public static PromotionExclusion Create(Guid promotionId, Guid excludedPromotionId)
    {
        return new PromotionExclusion
        {
            PromotionId = promotionId,
            ExcludedPromotionId = excludedPromotionId,
        };
    }
}
