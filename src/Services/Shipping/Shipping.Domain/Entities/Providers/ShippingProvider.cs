namespace NovaCore.Shipping.Domain.Entities.Providers;

/// <summary>
/// Who can physically carry a Transportation - an external carrier (GHN/VNPost/DHL), the
/// company's own internal fleet, or a freelancer. This replaces the narrower "Carrier" concept:
/// a Carrier implies a third-party logistics company, while ShippingProvider covers internal and
/// freelance capacity with the same model, which is what makes the service reusable for
/// warehouse transfers and supplier imports rather than just customer deliveries.
/// </summary>
public sealed class ShippingProvider : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ProviderType ProviderType { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>1:1 contact/coverage detail, shared-PK - see ShippingProviderProfile.</summary>
    public ShippingProviderProfile? Profile { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ShippingProvider() { }

    public static ShippingProvider Create(string code, string name, ProviderType providerType)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw ExceptionFactory.RequiredField("Provider code cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Provider name cannot be empty.");

        return new ShippingProvider
        {
            Id = Guid.CreateVersion7(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ProviderType = providerType,
            IsActive = true,
        };
    }

    /// <summary>Creates or replaces the provider's 1:1 profile - only ShippingProvider may construct one.</summary>
    public void SetProfile(
        string contactName,
        PhoneNumber contactPhone,
        Email? contactEmail = null,
        ShippingAddress? officeAddress = null,
        string? serviceAreas = null,
        string? note = null)
    {
        Profile = ShippingProviderProfile.Create(Id, contactName, contactPhone, contactEmail, officeAddress, serviceAreas, note);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Provider name cannot be empty.");

        Name = name.Trim();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
