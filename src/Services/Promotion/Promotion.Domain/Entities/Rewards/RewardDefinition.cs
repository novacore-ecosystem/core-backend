namespace NovaCore.Promotion.Domain.Entities.Rewards;

/// <summary>A structural reward-shape definition under a RewardProgram - Configuration is an opaque string blob, no schema/parsing logic lives here.</summary>
public sealed class RewardDefinition : BaseEntity<Guid>, IAuditable
{
    public Guid ProgramId { get; private set; }
    public RewardType RewardType { get; private set; }
    public string? Configuration { get; private set; }

    public RewardProgram Program { get; private set; } = default!;

    private RewardDefinition() { }

    /// <summary>Only RewardProgram may construct a RewardDefinition - see RewardProgram.AddDefinition.</summary>
    internal static RewardDefinition Create(Guid programId, RewardType rewardType, string? configuration)
    {
        return new RewardDefinition
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            RewardType = rewardType,
            Configuration = configuration,
        };
    }

    public void UpdateConfiguration(string? configuration)
    {
        Configuration = configuration;
    }
}
