namespace NovaCore.Promotion.Domain.Entities.Rewards;

/// <summary>A per-user execution record for a RewardDistribution - not navigated from RewardProgram, but is navigated from RewardDistribution as of Phase 3.1; construction remains public (RewardDistribution has no factory method for these). No dispatch/idempotency enforcement lives here.</summary>
public sealed class RewardExecution : BaseEntity<Guid>, IAuditable
{
    public Guid DistributionId { get; private set; }
    public Guid UserId { get; private set; }
    public string ExecutionKey { get; private set; } = string.Empty;
    public string? Status { get; private set; }
    public DateTime? ExecutedAt { get; private set; }

    public RewardDistribution Distribution { get; private set; } = default!;

    private RewardExecution() { }

    public static RewardExecution Create(Guid distributionId, Guid userId, string executionKey)
    {
        if (string.IsNullOrWhiteSpace(executionKey))
            throw ExceptionFactory.RequiredField("Execution key cannot be empty.");

        return new RewardExecution
        {
            Id = Guid.CreateVersion7(),
            DistributionId = distributionId,
            UserId = userId,
            ExecutionKey = executionKey,
        };
    }

    public void MarkExecuted(string status)
    {
        Status = status;
        ExecutedAt = DateTime.UtcNow;
    }
}
