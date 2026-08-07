namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>A structural (type, value) restriction on a Promotion (e.g. minimum order amount) - no validation/enforcement logic lives here.</summary>
public sealed class PromotionConstraint : BaseEntity<Guid>, IAuditable
{
    public Guid PromotionId { get; private set; }
    public PromotionConstraintType ConstraintType { get; private set; }
    public string Value { get; private set; } = string.Empty;

    public PromotionEntity Promotion { get; private set; } = default!;

    private PromotionConstraint() { }

    /// <summary>Only Promotion may construct a PromotionConstraint - see Promotion.AddConstraint.</summary>
    internal static PromotionConstraint Create(Guid promotionId, PromotionConstraintType constraintType, string value)
    {
        return new PromotionConstraint
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            ConstraintType = constraintType,
            Value = value,
        };
    }
}
