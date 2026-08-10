namespace NovaCore.Payment.Domain.Entities.Operations;

/// <summary>An outgoing payment from the platform to a payee (e.g. a merchant/seller payout).</summary>
public sealed class Payout : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid PayeeReferenceId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public PayoutStatus Status { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Payout() { }

    public static Payout Create(Guid payeeReferenceId, Money amount, DateTime? scheduledAt = null)
    {
        return new Payout
        {
            Id = Guid.CreateVersion7(),
            PayeeReferenceId = payeeReferenceId,
            Amount = amount,
            Status = PayoutStatus.Pending,
            ScheduledAt = scheduledAt,
        };
    }
}
