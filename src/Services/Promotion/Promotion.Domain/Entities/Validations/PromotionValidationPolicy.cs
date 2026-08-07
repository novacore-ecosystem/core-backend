namespace NovaCore.Promotion.Domain.Entities.Validations;

/// <summary>
/// A structural validation rule definition - no Properties/Navigation/TenantId was given for this
/// entity or any other in the Validation group, unlike every prior aggregate root, so it is kept
/// as a plain BaseEntity (not AggregateRoot, no ITenantEntity) rather than inventing an unrequested
/// shape - see docs/promotion-service/aggregates/validation.md. Configuration is an opaque string
/// blob, no rule evaluation logic lives here.
/// </summary>
public sealed class PromotionValidationPolicy : BaseEntity<Guid>, IAuditable
{
    public string Name { get; private set; } = string.Empty;
    public string RuleType { get; private set; } = string.Empty;
    public string? Configuration { get; private set; }
    public int Priority { get; private set; }

    public ICollection<PromotionValidationResult> Results { get; private set; } = [];

    private PromotionValidationPolicy() { }

    public static PromotionValidationPolicy Create(string name, string ruleType, string? configuration = null, int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Policy name cannot be empty.");

        if (string.IsNullOrWhiteSpace(ruleType))
            throw ExceptionFactory.RequiredField("Rule type cannot be empty.");

        return new PromotionValidationPolicy
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            RuleType = ruleType,
            Configuration = configuration,
            Priority = priority,
        };
    }

    public void UpdateConfiguration(string? configuration)
    {
        Configuration = configuration;
    }
}
