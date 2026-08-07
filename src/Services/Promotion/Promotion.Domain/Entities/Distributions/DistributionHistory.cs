namespace NovaCore.Promotion.Domain.Entities.Distributions;

/// <summary>An append-only audit trail row for a DistributionJob - CreatedAt is inherited from BaseEntity, not redeclared. Construction remains public (DistributionJob has no factory method for these).</summary>
public sealed class DistributionHistory : BaseEntity<Guid>, IAuditable
{
    public Guid JobId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? OperatorId { get; private set; }

    public DistributionJob Job { get; private set; } = default!;

    private DistributionHistory() { }

    public static DistributionHistory Create(Guid jobId, string action, Guid? operatorId)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw ExceptionFactory.RequiredField("History action cannot be empty.");

        return new DistributionHistory
        {
            Id = Guid.CreateVersion7(),
            JobId = jobId,
            Action = action,
            OperatorId = operatorId,
        };
    }
}
