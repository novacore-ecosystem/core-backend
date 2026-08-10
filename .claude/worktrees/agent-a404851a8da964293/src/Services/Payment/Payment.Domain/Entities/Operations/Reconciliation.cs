namespace NovaCore.Payment.Domain.Entities.Operations;

/// <summary>Financial reconciliation record against a Settlement - flags discrepancies between what the gateway reported and what this service recorded.</summary>
public sealed class Reconciliation : AggregateRoot<Guid>, IAuditable
{
    public Guid SettlementId { get; private set; }
    public SettlementStatus Status { get; private set; }
    public decimal DiscrepancyAmount { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? ReconciledAt { get; private set; }

    private Reconciliation() { }

    public static Reconciliation Create(Guid settlementId, decimal discrepancyAmount = 0, string? notes = null)
    {
        return new Reconciliation
        {
            Id = Guid.CreateVersion7(),
            SettlementId = settlementId,
            Status = SettlementStatus.Pending,
            DiscrepancyAmount = discrepancyAmount,
            Notes = notes,
        };
    }
}
