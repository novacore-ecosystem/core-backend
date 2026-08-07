namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>A single point movement for a PointAccount - construction remains public (PointAccount has no factory method for these). No earn/spend calculation lives here.</summary>
public sealed class PointTransaction : BaseEntity<Guid>, IAuditable
{
    public Guid AccountId { get; private set; }
    public PointTransactionType Type { get; private set; }
    public string? Source { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public int Points { get; private set; }
    public int BalanceAfter { get; private set; }

    public PointAccount Account { get; private set; } = default!;
    public ICollection<PointLedger> Ledgers { get; private set; } = [];

    private PointTransaction() { }

    public static PointTransaction Create(Guid accountId, PointTransactionType type, int points, int balanceAfter, string? source = null, Guid? referenceId = null)
    {
        return new PointTransaction
        {
            Id = Guid.CreateVersion7(),
            AccountId = accountId,
            Type = type,
            Source = source,
            ReferenceId = referenceId,
            Points = points,
            BalanceAfter = balanceAfter,
        };
    }
}
