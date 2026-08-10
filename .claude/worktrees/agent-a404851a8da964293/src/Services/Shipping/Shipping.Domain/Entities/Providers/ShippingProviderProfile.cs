namespace NovaCore.Shipping.Domain.Entities.Providers;

/// <summary>
/// Contact and coverage detail of a ShippingProvider. Strict 1:1 extension, so its primary key
/// *is* ProviderId (no surrogate id) per the shared-PK convention.
/// </summary>
public sealed class ShippingProviderProfile : BaseEntity, IAuditable
{
    /// <summary>The primary key - shared with ShippingProvider, not a surrogate.</summary>
    public Guid ProviderId { get; private set; }
    public string ContactName { get; private set; } = string.Empty;
    public PhoneNumber ContactPhone { get; private set; } = default!;
    public Email? ContactEmail { get; private set; }
    public ShippingAddress? OfficeAddress { get; private set; }

    /// <summary>Free-text coverage description (province/district list). Structured service-area matching is a later phase - nothing evaluates this yet.</summary>
    public string? ServiceAreas { get; private set; }
    public string? Note { get; private set; }

    private ShippingProviderProfile() { }

    internal static ShippingProviderProfile Create(
        Guid providerId,
        string contactName,
        PhoneNumber contactPhone,
        Email? contactEmail = null,
        ShippingAddress? officeAddress = null,
        string? serviceAreas = null,
        string? note = null)
    {
        if (string.IsNullOrWhiteSpace(contactName))
            throw ExceptionFactory.RequiredField("Provider contact name cannot be empty.");

        return new ShippingProviderProfile
        {
            ProviderId = providerId,
            ContactName = contactName.Trim(),
            ContactPhone = contactPhone,
            ContactEmail = contactEmail,
            OfficeAddress = officeAddress,
            ServiceAreas = serviceAreas?.Trim(),
            Note = note?.Trim(),
        };
    }
}
