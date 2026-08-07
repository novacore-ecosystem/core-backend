namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>What a Promotion grants (e.g. a discount amount/percentage) - structural (type, value) pair, no discount calculation lives here.</summary>
public sealed class PromotionBenefit : BaseEntity<Guid>, IAuditable
{
    public Guid PromotionId { get; private set; }
    public PromotionBenefitType BenefitType { get; private set; }
    public decimal Value { get; private set; }
    public int DisplayOrder { get; private set; }

    private PromotionBenefit() { }

    /// <summary>Only Promotion may construct a PromotionBenefit - see Promotion.AddBenefit.</summary>
    internal static PromotionBenefit Create(Guid promotionId, PromotionBenefitType benefitType, decimal value, int displayOrder)
    {
        return new PromotionBenefit
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            BenefitType = benefitType,
            Value = value,
            DisplayOrder = displayOrder,
        };
    }
}
