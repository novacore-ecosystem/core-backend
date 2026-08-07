namespace NovaCore.Promotion.Domain.Entities.Loyalty;

/// <summary>A double-entry ledger row for a PointTransaction - construction remains public (PointTransaction has no factory method for these). No balance calculation lives here.</summary>
public sealed class PointLedger : BaseEntity<Guid>, IAuditable
{
    public Guid TransactionId { get; private set; }
    public int Debit { get; private set; }
    public int Credit { get; private set; }
    public int Balance { get; private set; }

    public PointTransaction Transaction { get; private set; } = default!;

    private PointLedger() { }

    public static PointLedger Create(Guid transactionId, int debit, int credit, int balance)
    {
        return new PointLedger
        {
            Id = Guid.CreateVersion7(),
            TransactionId = transactionId,
            Debit = debit,
            Credit = credit,
            Balance = balance,
        };
    }
}
