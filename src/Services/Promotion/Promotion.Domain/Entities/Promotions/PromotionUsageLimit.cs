namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>A structural usage cap for a Promotion at a given scope (e.g. "PerCustomer", "PerOrder", "Global") - no usage counting/enforcement lives here.</summary>
public sealed class PromotionUsageLimit : BaseEntity<Guid>, IAuditable
{
    public Guid PromotionId { get; private set; }
    public PromotionUsageScope Scope { get; private set; }
    public int MaxUsage { get; private set; }

    private PromotionUsageLimit() { }

    /// <summary>Only Promotion may construct a PromotionUsageLimit - see Promotion.AddUsageLimit.</summary>
    internal static PromotionUsageLimit Create(Guid promotionId, PromotionUsageScope scope, int maxUsage)
    {
        if (maxUsage < 0)
            throw ExceptionFactory.InvalidRange("Max usage cannot be negative.");

        return new PromotionUsageLimit
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            Scope = scope,
            MaxUsage = maxUsage,
        };
    }
}
