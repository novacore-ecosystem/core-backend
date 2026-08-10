namespace NovaCore.Payment.Domain.Entities.Scheduling;

/// <summary>Recurring payment schedule against ReferenceType/ReferenceId (e.g. a subscription's billing cycle). Execution/dispatch logic is a later phase - this only tracks the schedule itself.</summary>
public sealed class ScheduledPayment : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public ReferenceType ReferenceType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public PaymentFrequency Frequency { get; private set; }
    public DateTime NextRunAt { get; private set; }
    public ScheduledPaymentStatus Status { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ScheduledPayment() { }

    public static ScheduledPayment Create(ReferenceType referenceType, Guid referenceId, Money amount, PaymentFrequency frequency, DateTime nextRunAt)
    {
        return new ScheduledPayment
        {
            Id = Guid.CreateVersion7(),
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Amount = amount,
            Frequency = frequency,
            NextRunAt = nextRunAt,
            Status = ScheduledPaymentStatus.Active,
        };
    }
}
