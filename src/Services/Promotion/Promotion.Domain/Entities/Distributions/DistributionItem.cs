namespace NovaCore.Promotion.Domain.Entities.Distributions;

/// <summary>A single user/reward-reference item within a DistributionBatch - reuses the Reward aggregate's RewardType enum (field name/semantics line up exactly, both implemented in this same phase). Not navigated from DistributionJob directly, but is navigated from DistributionBatch as of Phase 3.1; construction remains public (DistributionBatch has no factory method for these).</summary>
public sealed class DistributionItem : BaseEntity<Guid>, IAuditable
{
    public Guid BatchId { get; private set; }
    public Guid UserId { get; private set; }
    public RewardType RewardType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? Status { get; private set; }

    public DistributionBatch Batch { get; private set; } = default!;
    public ICollection<DistributionExecution> Executions { get; private set; } = [];

    private DistributionItem() { }

    public static DistributionItem Create(Guid batchId, Guid userId, RewardType rewardType, Guid? referenceId = null)
    {
        return new DistributionItem
        {
            Id = Guid.CreateVersion7(),
            BatchId = batchId,
            UserId = userId,
            RewardType = rewardType,
            ReferenceId = referenceId,
        };
    }

    public void UpdateStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw ExceptionFactory.RequiredField("Item status cannot be empty.");

        Status = status;
    }
}
