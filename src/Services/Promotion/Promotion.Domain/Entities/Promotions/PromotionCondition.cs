namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>A single structural (field, operator, value) condition triple belonging to a PromotionRule. No evaluation/comparison logic lives here - purely a stored condition row.</summary>
public sealed class PromotionCondition : BaseEntity<Guid>, IAuditable
{
    public Guid RuleId { get; private set; }
    public string Field { get; private set; } = string.Empty;
    public PromotionConditionOperator Operator { get; private set; }
    public string Value { get; private set; } = string.Empty;

    public PromotionRule Rule { get; private set; } = default!;

    private PromotionCondition() { }

    /// <summary>Only PromotionRule constructs a PromotionCondition - see PromotionRule.AddCondition.</summary>
    internal static PromotionCondition Create(Guid ruleId, string field, PromotionConditionOperator operatorSymbol, string value)
    {
        if (string.IsNullOrWhiteSpace(field))
            throw ExceptionFactory.RequiredField("Condition field cannot be empty.");

        return new PromotionCondition
        {
            Id = Guid.CreateVersion7(),
            RuleId = ruleId,
            Field = field,
            Operator = operatorSymbol,
            Value = value,
        };
    }
}
