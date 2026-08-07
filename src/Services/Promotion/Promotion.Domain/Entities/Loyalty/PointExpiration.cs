namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>A record of points expiring for a PointAccount - construction remains public (PointAccount has no factory method for these). No expiry sweeping logic lives here.</summary>
public sealed class PointExpiration : BaseEntity<Guid>, IAuditable
{
    public Guid AccountId { get; private set; }
    public int Points { get; private set; }
    public DateTime ExpiredAt { get; private set; }

    public PointAccount Account { get; private set; } = default!;

    private PointExpiration() { }

    public static PointExpiration Create(Guid accountId, int points)
    {
        return new PointExpiration
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            Points = points,
            ExpiredAt = DateTime.UtcNow,
        };
    }
}
