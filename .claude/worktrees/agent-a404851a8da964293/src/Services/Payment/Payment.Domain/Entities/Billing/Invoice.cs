namespace NovaCore.Payment.Domain.Entities.Billing;

/// <summary>Payment invoice against ReferenceType/ReferenceId. Must not depend on Order - integrates purely through the reference pair, like Payment.</summary>
public sealed class Invoice : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public ReferenceType ReferenceType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public Guid? BillingProfileId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public InvoiceStatus Status { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Invoice() { }

    public static Invoice Create(
        ReferenceType referenceType,
        Guid referenceId,
        Money amount,
        Guid? billingProfileId = null,
        DateTime? dueDate = null)
    {
        return new Invoice
        {
            Id = Guid.CreateVersion7(),
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            BillingProfileId = billingProfileId,
            Amount = amount,
            Status = InvoiceStatus.Draft,
            DueDate = dueDate,
        };
    }
}
