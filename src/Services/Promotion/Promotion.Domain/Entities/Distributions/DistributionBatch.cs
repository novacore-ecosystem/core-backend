namespace NovaCore.Promotion.Domain.Entities.Distributions;

/// <summary>A chunk of a DistributionJob's work - no processing/orchestration logic lives here, see UpdateCounts.</summary>
public sealed class DistributionBatch : BaseEntity<Guid>, IAuditable
{
    public Guid JobId { get; private set; }
    public int BatchNo { get; private set; }
    public int TotalItems { get; private set; }
    public int ProcessedItems { get; private set; }
    public int FailedItems { get; private set; }

    public DistributionJob Job { get; private set; } = default!;
    public ICollection<DistributionItem> Items { get; private set; } = [];

    private DistributionBatch() { }

    /// <summary>Only DistributionJob may construct a DistributionBatch - see DistributionJob.AddBatch.</summary>
    internal static DistributionBatch Create(Guid jobId, int batchNo, int totalItems)
    {
        return new DistributionBatch
        {
            Id = Guid.CreateVersion7(),
            JobId = jobId,
            BatchNo = batchNo,
            TotalItems = totalItems,
        };
    }

    /// <summary>Structural counter update only - no processing logic lives here.</summary>
    public void UpdateCounts(int processedItems, int failedItems)
    {
        ProcessedItems = processedItems;
        FailedItems = failedItems;
    }
}
