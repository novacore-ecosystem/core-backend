using NovaCore.Promotion.Domain.Entities.Distributions;

namespace NovaCore.Promotion.Domain.Entities.Rewards;

/// <summary>
/// A distribution run for a RewardProgram, optionally tied to a DistributionJob (DistributionJobId).
/// Status reuses the Distribution aggregate's DistributionStatus enum - both are implemented in
/// this same phase and the field name/semantics line up exactly, so no separate RewardDistribution-
/// specific status enum was introduced.
/// </summary>
public sealed class RewardDistribution : BaseEntity<Guid>, IAuditable
{
    public Guid ProgramId { get; private set; }
    public Guid? DistributionJobId { get; private set; }
    public DistributionStatus Status { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? ExecutedAt { get; private set; }

    /// <summary>Forward reference to the independent DistributionJob root (Phase 2.6 correction) - same pattern as Coupon.Campaign/Coupon.Promotion, resolved at the Persistence layer via FK.</summary>
    public DistributionJob? DistributionJob { get; private set; }

    private RewardDistribution() { }

    /// <summary>Only RewardProgram may construct a RewardDistribution - see RewardProgram.AddDistribution.</summary>
    internal static RewardDistribution Create(Guid programId, Guid? distributionJobId, DateTime? scheduledAt)
    {
        return new RewardDistribution
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            DistributionJobId = distributionJobId,
            Status = DistributionStatus.Draft,
            ScheduledAt = scheduledAt,
        };
    }

    public void MarkExecuted()
    {
        ExecutedAt = DateTime.UtcNow;
        Status = DistributionStatus.Completed;
    }

    public void UpdateStatus(DistributionStatus status)
    {
        Status = status;
    }
}
