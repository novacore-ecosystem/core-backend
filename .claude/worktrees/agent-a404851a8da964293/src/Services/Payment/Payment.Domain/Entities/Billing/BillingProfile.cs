namespace NovaCore.Payment.Domain.Entities.Billing;

/// <summary>Billing information for an owner (individual or organization), used by Invoice. PaymentService-owned, never depends on Order/User entities directly.</summary>
public sealed class BillingProfile : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid OwnerReferenceId { get; private set; }
    public string LegalName { get; private set; } = string.Empty;
    public string? TaxId { get; private set; }
    public BillingAddress Address { get; private set; } = default!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private BillingProfile() { }

    public static BillingProfile Create(
        Guid ownerReferenceId,
        string legalName,
        BillingAddress address,
        string? taxId = null,
        string? email = null,
        string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(legalName))
            throw ExceptionFactory.RequiredField("Billing profile legal name cannot be empty.");

        return new BillingProfile
        {
            Id = Guid.CreateVersion7(),
            OwnerReferenceId = ownerReferenceId,
            LegalName = legalName.Trim(),
            Address = address,
            TaxId = taxId,
            Email = email,
            Phone = phone,
        };
    }
}
