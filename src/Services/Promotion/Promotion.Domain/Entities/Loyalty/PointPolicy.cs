namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>A structural policy configuration under a LoyaltyProgram (e.g. tier thresholds, redemption limits) - Configuration is an opaque string blob, no schema/parsing logic lives here.</summary>
public sealed class PointPolicy : BaseEntity<Guid>, IAuditable
{
    public Guid ProgramId { get; private set; }
    public string PolicyType { get; private set; } = string.Empty;
    public string? Configuration { get; private set; }

    public LoyaltyProgram Program { get; private set; } = default!;

    private PointPolicy() { }

    /// <summary>Only LoyaltyProgram may construct a PointPolicy - see LoyaltyProgram.AddPointPolicy.</summary>
    internal static PointPolicy Create(Guid programId, string policyType, string? configuration)
    {
        if (string.IsNullOrWhiteSpace(policyType))
            throw ExceptionFactory.RequiredField("Policy type cannot be empty.");

        return new PointPolicy
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            PolicyType = policyType,
            Configuration = configuration,
        };
    }

    public void UpdateConfiguration(string? configuration)
    {
        Configuration = configuration;
    }
}
