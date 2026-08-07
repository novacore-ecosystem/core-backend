namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>A structural earn/spend rule under a LoyaltyProgram - plain string RuleType (no enum requested), no rule evaluation logic lives here.</summary>
public sealed class PointRule : BaseEntity<Guid>, IAuditable
{
    public Guid ProgramId { get; private set; }
    public string RuleType { get; private set; } = string.Empty;
    public int Priority { get; private set; }
    public bool IsEnabled { get; private set; }

    public LoyaltyProgram Program { get; private set; } = default!;

    private PointRule() { }

    /// <summary>Only LoyaltyProgram may construct a PointRule - see LoyaltyProgram.AddPointRule.</summary>
    internal static PointRule Create(Guid programId, string ruleType, int priority)
    {
        if (string.IsNullOrWhiteSpace(ruleType))
            throw ExceptionFactory.RequiredField("Rule type cannot be empty.");

        return new PointRule
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            RuleType = ruleType,
            Priority = priority,
            IsEnabled = true,
        };
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;
}
