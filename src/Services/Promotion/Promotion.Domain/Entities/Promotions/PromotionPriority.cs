namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>
/// A structural priority classification record for a Promotion - distinct from the plain
/// ordering int on Promotion.Priority; this captures a typed weight (PromotionPriorityType +
/// PromotionPriorityValue) for a specific evaluation context. Related to Promotion via
/// PromotionId only (not a Promotion navigation collection). No conflict-resolution logic lives
/// here.
/// </summary>
public sealed class PromotionPriority : BaseEntity<Guid>, IAuditable
{
    public Guid PromotionId { get; private set; }
    public PromotionPriorityType PriorityType { get; private set; }
    public PromotionPriorityValue Value { get; private set; } = default!;
    public string? Note { get; private set; }

    private PromotionPriority() { }

    public static PromotionPriority Create(Guid promotionId, PromotionPriorityType priorityType, PromotionPriorityValue value, string? note)
    {
        return new PromotionPriority
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            PriorityType = priorityType,
            Value = value,
            Note = note,
        };
    }
}
