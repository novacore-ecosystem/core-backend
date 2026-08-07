namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>
/// Groups several PromotionRule rows under one AND/OR combinator. Related to Promotion via
/// PromotionId only (not a Promotion navigation collection - PromotionRule references it via
/// RuleGroupId instead). No rule evaluation lives here - LogicOperator is a structural label.
/// </summary>
public sealed class PromotionRuleGroup : BaseEntity<Guid>, IAuditable
{
    public Guid PromotionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public PromotionRuleGroupOperator LogicOperator { get; private set; } = PromotionRuleGroupOperator.And;
    public int DisplayOrder { get; private set; }

    private PromotionRuleGroup() { }

    public static PromotionRuleGroup Create(Guid promotionId, string name, PromotionRuleGroupOperator logicOperator, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Rule group name cannot be empty.");

        return new PromotionRuleGroup
        {
            Id = Guid.CreateVersion7(),
            PromotionId = promotionId,
            Name = name,
            LogicOperator = logicOperator,
            DisplayOrder = displayOrder,
        };
    }
}
