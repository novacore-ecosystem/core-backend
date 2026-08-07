namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>Strict 1:1 detail table for how a Promotion combines with other active promotions - reuses the parent's Id, no surrogate key (Rule 5).</summary>
public sealed class PromotionStackingPolicy : BaseEntity, IAuditable
{
    public Guid PromotionId { get; private set; }
    public PromotionStackingMode Mode { get; private set; }
    public string? Note { get; private set; }

    public PromotionEntity Promotion { get; private set; } = default!;

    private PromotionStackingPolicy() { }

    /// <summary>Only Promotion may construct a PromotionStackingPolicy - see Promotion.SetStackingPolicy.</summary>
    internal static PromotionStackingPolicy Create(Guid promotionId, PromotionStackingMode mode, string? note)
    {
        return new PromotionStackingPolicy
        {
            PromotionId = promotionId,
            Mode = mode,
            Note = note,
        };
    }

    internal void UpdateDetails(PromotionStackingMode mode, string? note)
    {
        Mode = mode;
        Note = note;
    }
}
