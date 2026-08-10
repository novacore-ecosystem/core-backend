namespace NovaCore.Shipping.Domain.Entities.ShippingProfiles;

/// <summary>
/// A user's saved sender/receiver preset ("Home", "Office"), used to auto-complete a shipment
/// form. Purely a convenience record owned by ShippingService - UserService still owns the user's
/// canonical address book; this is the shipping-side, shipping-shaped copy that also carries a
/// verification state and last-used tracking.
/// </summary>
public sealed class ShippingProfile : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string ContactName { get; private set; } = string.Empty;
    public PhoneNumber ContactPhone { get; private set; } = default!;
    public ShippingAddress Address { get; private set; } = default!;
    public bool IsDefault { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; }

    /// <summary>Set when this profile's address was matched to a VerifiedShippingAddress - null while unverified.</summary>
    public Guid? VerifiedAddressId { get; private set; }
    public DateTime? LastUsedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ShippingProfile() { }

    public static ShippingProfile Create(
        Guid userId,
        string label,
        string contactName,
        PhoneNumber contactPhone,
        ShippingAddress address,
        bool isDefault = false)
    {
        if (userId == Guid.Empty)
            throw ExceptionFactory.RequiredField("User id is required.");

        if (string.IsNullOrWhiteSpace(label))
            throw ExceptionFactory.RequiredField("Shipping profile label cannot be empty.");

        if (string.IsNullOrWhiteSpace(contactName))
            throw ExceptionFactory.RequiredField("Contact name cannot be empty.");

        return new ShippingProfile
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Label = label.Trim(),
            ContactName = contactName.Trim(),
            ContactPhone = contactPhone,
            Address = address,
            IsDefault = isDefault,
            VerificationStatus = VerificationStatus.Unverified,
        };
    }

    public void UpdateContact(string contactName, PhoneNumber contactPhone, ShippingAddress address)
    {
        if (string.IsNullOrWhiteSpace(contactName))
            throw ExceptionFactory.RequiredField("Contact name cannot be empty.");

        ContactName = contactName.Trim();
        ContactPhone = contactPhone;
        Address = address;

        // Any address change invalidates a previous verification - the new address has not been
        // checked against a VerifiedShippingAddress yet.
        VerificationStatus = VerificationStatus.Unverified;
        VerifiedAddressId = null;
    }

    public void MarkAsDefault() => IsDefault = true;

    public void UnmarkAsDefault() => IsDefault = false;

    public void MarkVerified(Guid verifiedAddressId)
    {
        if (verifiedAddressId == Guid.Empty)
            throw ExceptionFactory.RequiredField("A verified address id is required.");

        VerifiedAddressId = verifiedAddressId;
        VerificationStatus = VerificationStatus.Verified;
    }

    public void MarkUsed() => LastUsedAt = DateTime.UtcNow;
}
