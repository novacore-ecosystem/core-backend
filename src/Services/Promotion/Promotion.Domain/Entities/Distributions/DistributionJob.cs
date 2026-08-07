namespace NovaCore.Promotion.Domain.Entities.Distributions;

/// <summary>
/// Aggregate root for a DistributionJob - owns Batches. DistributionItem/DistributionExecution/
/// DistributionRetry/DistributionHistory are related by id only, not navigated from here - see
/// docs/promotion-service/aggregates/distribution.md. Code now uses the shared EntityCode Value
/// Object, same as every other aggregate root (Phase 2.6 correction).
/// </summary>
public sealed class DistributionJob : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public DistributionStatus Status { get; private set; }
    public DistributionStrategy Strategy { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public ICollection<DistributionBatch> Batches { get; private set; } = [];
    public ICollection<DistributionHistory> History { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private DistributionJob() { }

    public static DistributionJob Create(EntityCode code, string name, DistributionStrategy strategy, DateTime? scheduledAt = null)
    {
        ValidateName(name);

        return new DistributionJob
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Status = DistributionStatus.Draft,
            Strategy = strategy,
            ScheduledAt = scheduledAt,
        };
    }
    #endregion

    #region Details & lifecycle
    public void UpdateDetails(string name)
    {
        ValidateName(name);

        Name = name;
    }

    public void Reschedule(DateTime scheduledAt)
    {
        ScheduledAt = scheduledAt;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Distribution job name cannot be empty.");
    }
    #endregion

    #region Status
    public void Schedule()
    {
        if (Status != DistributionStatus.Draft)
            throw ExceptionFactory.InvalidStatus($"Cannot schedule a distribution job in {Status} status.");

        Status = DistributionStatus.Scheduled;
    }

    public void Start()
    {
        if (Status is not (DistributionStatus.Scheduled or DistributionStatus.Paused))
            throw ExceptionFactory.InvalidStatus($"Cannot start a distribution job in {Status} status.");

        Status = DistributionStatus.Running;
        StartedAt ??= DateTime.UtcNow;
    }

    public void Pause()
    {
        if (Status != DistributionStatus.Running)
            throw ExceptionFactory.InvalidStatus($"Cannot pause a distribution job in {Status} status.");

        Status = DistributionStatus.Paused;
    }

    public void Complete()
    {
        if (Status != DistributionStatus.Running)
            throw ExceptionFactory.InvalidStatus($"Cannot complete a distribution job in {Status} status.");

        Status = DistributionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status is DistributionStatus.Completed or DistributionStatus.Cancelled or DistributionStatus.Failed)
            throw ExceptionFactory.InvalidStatus($"Cannot cancel a distribution job in {Status} status.");

        Status = DistributionStatus.Cancelled;
    }

    public void Fail()
    {
        if (Status is DistributionStatus.Completed or DistributionStatus.Cancelled)
            throw ExceptionFactory.InvalidStatus($"Cannot fail a distribution job in {Status} status.");

        Status = DistributionStatus.Failed;
    }
    #endregion

    #region Batch
    public void AddBatch(int batchNo, int totalItems)
    {
        Batches.Add(DistributionBatch.Create(Id, batchNo, totalItems));
    }

    public void RemoveBatch(Guid batchId)
    {
        var batch = Batches.FirstOrDefault(b => b.Id == batchId)
            ?? throw ExceptionFactory.EntityNotFound<DistributionBatch>(batchId);

        Batches.Remove(batch);
    }
    #endregion
}
