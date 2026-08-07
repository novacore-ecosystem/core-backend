namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>Strict 1:1 detail table for how a Promotion is triggered - reuses the parent's Id, no surrogate key (Rule 5).</summary>
public sealed class PromotionExecutionPolicy : BaseEntity, IAuditable
{
    public Guid PromotionId { get; private set; }
    public PromotionExecutionMode Mode { get; private set; }
    public int? MaxExecutionsPerOrder { get; private set; }
    public string? Note { get; private set; }

    public PromotionEntity Promotion { get; private set; } = default!;

    private PromotionExecutionPolicy() { }

    /// <summary>Only Promotion may construct a PromotionExecutionPolicy - see Promotion.SetExecutionPolicy.</summary>
    internal static PromotionExecutionPolicy Create(Guid promotionId, PromotionExecutionMode mode, int? maxExecutionsPerOrder, string? note)
    {
        return new PromotionExecutionPolicy
        {
            PromotionId = promotionId,
            Mode = mode,
            MaxExecutionsPerOrder = maxExecutionsPerOrder,
            Note = note,
        };
    }

    internal void UpdateDetails(PromotionExecutionMode mode, int? maxExecutionsPerOrder, string? note)
    {
        Mode = mode;
        MaxExecutionsPerOrder = maxExecutionsPerOrder;
        Note = note;
    }
}
