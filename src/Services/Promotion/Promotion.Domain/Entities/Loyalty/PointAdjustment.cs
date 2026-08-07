namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>A manual point correction for a PointAccount - construction remains public (PointAccount has no factory method for these). No approval/validation logic lives here.</summary>
public sealed class PointAdjustment : BaseEntity<Guid>, IAuditable
{
    public Guid AccountId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public int Points { get; private set; }
    public Guid? OperatorId { get; private set; }
    public DateTime AdjustedAt { get; private set; }

    public PointAccount Account { get; private set; } = default!;

    private PointAdjustment() { }

    public static PointAdjustment Create(Guid accountId, string reason, int points, Guid? operatorId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw ExceptionFactory.RequiredField("Adjustment reason cannot be empty.");

        return new PointAdjustment
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            Reason = reason,
            Points = points,
            OperatorId = operatorId,
            AdjustedAt = DateTime.UtcNow,
        };
    }
}
