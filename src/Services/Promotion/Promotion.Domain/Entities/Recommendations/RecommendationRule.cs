namespace NovaCore.Promotion.Domain.Entities.Recommendations;

/// <summary>A structural rule under a RecommendationProgram - Configuration is an opaque string blob, no rule evaluation logic lives here.</summary>
public sealed class RecommendationRule : BaseEntity<Guid>, IAuditable
{
    public Guid ProgramId { get; private set; }
    public string RuleType { get; private set; } = string.Empty;
    public string? Configuration { get; private set; }
    public int Priority { get; private set; }

    public RecommendationProgram Program { get; private set; } = default!;

    private RecommendationRule() { }

    /// <summary>Only RecommendationProgram may construct a RecommendationRule - see RecommendationProgram.AddRule.</summary>
    internal static RecommendationRule Create(Guid programId, string ruleType, string? configuration, int priority)
    {
        if (string.IsNullOrWhiteSpace(ruleType))
            throw ExceptionFactory.RequiredField("Rule type cannot be empty.");

        return new RecommendationRule
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            RuleType = ruleType,
            Configuration = configuration,
            Priority = priority,
        };
    }
}
