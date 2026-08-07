namespace NovaCore.Promotion.Domain.Entities.Distributions;

/// <summary>A retry counter for a DistributionExecution - not navigated from DistributionJob directly, but is navigated from DistributionExecution as of Phase 3.1; construction remains public (DistributionExecution has no factory method for these). No retry-scheduling/backoff logic lives here.</summary>
public sealed class DistributionRetry : BaseEntity<Guid>, IAuditable
{
    public Guid ExecutionId { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? LastRetryAt { get; private set; }

    public DistributionExecution Execution { get; private set; } = default!;

    private DistributionRetry() { }

    public static DistributionRetry Create(Guid executionId)
    {
        return new DistributionRetry
        {
            Id = Guid.CreateVersion7(),
            ExecutionId = executionId,
        };
    }

    /// <summary>Structural counter increment only - no backoff/scheduling logic lives here.</summary>
    public void RecordRetry()
    {
        RetryCount++;
        LastRetryAt = DateTime.UtcNow;
    }
}
