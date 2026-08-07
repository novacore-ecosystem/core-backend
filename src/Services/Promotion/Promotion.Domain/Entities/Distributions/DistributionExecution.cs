namespace NovaCore.Promotion.Domain.Entities.Distributions;

/// <summary>A dispatch attempt record for a DistributionItem - not navigated from DistributionJob directly, but is navigated from DistributionItem as of Phase 3.1; construction remains public (DistributionItem has no factory method for these). No dispatch/idempotency enforcement lives here.</summary>
public sealed class DistributionExecution : BaseEntity<Guid>, IAuditable
{
    public Guid ItemId { get; private set; }
    public string ExecutionKey { get; private set; } = string.Empty;
    public DateTime? ExecutedAt { get; private set; }

    public DistributionItem Item { get; private set; } = default!;
    public ICollection<DistributionRetry> Retries { get; private set; } = [];

    private DistributionExecution() { }

    public static DistributionExecution Create(Guid itemId, string executionKey)
    {
        if (string.IsNullOrWhiteSpace(executionKey))
            throw ExceptionFactory.RequiredField("Execution key cannot be empty.");

        return new DistributionExecution
        {
            Id = Guid.CreateVersion7(),
            ItemId = itemId,
            ExecutionKey = executionKey,
        };
    }

    public void MarkExecuted()
    {
        ExecutedAt = DateTime.UtcNow;
    }
}
