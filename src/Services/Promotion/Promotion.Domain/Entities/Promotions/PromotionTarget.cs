namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>What a Promotion applies to (e.g. a Product/Category/Cart reference) - structural (type, key) pair, no eligibility resolution lives here.</summary>
public sealed class PromotionTarget : BaseEntity<Guid>, IAuditable
{
    public Guid PromotionId { get; private set; }
    public PromotionTargetType TargetType { get; private set; }
    public string TargetKey { get; private set; } = string.Empty;

    public PromotionEntity Promotion { get; private set; } = default!;

    private PromotionTarget() { }

    /// <summary>Only Promotion may construct a PromotionTarget - see Promotion.AddTarget.</summary>
    internal static PromotionTarget Create(Guid promotionId, PromotionTargetType targetType, string targetKey)
    {
        if (string.IsNullOrWhiteSpace(targetKey))
            throw ExceptionFactory.RequiredField("Target key cannot be empty.");

        return new PromotionTarget
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            TargetType = targetType,
            TargetKey = targetKey,
        };
    }
}
