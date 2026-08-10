namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned child of User representing one saved delivery/billing location. A user may hold many;
/// "default shipping" and "default billing" are independent single-winner flags enforced by the
/// User aggregate root.
/// </summary>
public sealed class UserAddress : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public string Label { get; private set; } = string.Empty;
    public Receiver Receiver { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public GeoLocation? GeoLocation { get; private set; }
    public string? Building { get; private set; }
    public string? Apartment { get; private set; }
    public string? Floor { get; private set; }
    public string? DeliveryInstruction { get; private set; }
    public AddressType AddressType { get; private set; }
    public bool IsDefaultShipping { get; private set; }
    public bool IsDefaultBilling { get; private set; }
    public bool IsVerified { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserAddress() { }

    public static UserAddress Create(
        Guid userId,
        string label,
        Receiver receiver,
        Address address,
        AddressType addressType,
        GeoLocation? geoLocation = null,
        string? building = null,
        string? apartment = null,
        string? floor = null,
        string? deliveryInstruction = null,
        bool isDefaultShipping = false,
        bool isDefaultBilling = false)
    {
        ValidateLabel(label);

        return new UserAddress
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Label = label,
            Receiver = receiver,
            Address = address,
            GeoLocation = geoLocation,
            Building = building,
            Apartment = apartment,
            Floor = floor,
            DeliveryInstruction = deliveryInstruction,
            AddressType = addressType,
            IsDefaultShipping = isDefaultShipping,
            IsDefaultBilling = isDefaultBilling,
            IsVerified = false,
        };
    }

    // ============================================================================
    // Default flags
    // Manages the IsDefaultShipping/IsDefaultBilling toggles. The User aggregate
    // root is responsible for unmarking any previous default before calling the
    // corresponding Mark* method here, keeping the "at most one default per kind"
    // invariant centralized on User.
    // ============================================================================

    #region Default flags

    public void MarkAsDefaultShipping()
    {
        IsDefaultShipping = true;
    }

    public void UnmarkAsDefaultShipping()
    {
        IsDefaultShipping = false;
    }

    public void MarkAsDefaultBilling()
    {
        IsDefaultBilling = true;
    }

    public void UnmarkAsDefaultBilling()
    {
        IsDefaultBilling = false;
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core address fields, geo-location, verification flag, and the shared
    // label-validation rule.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateDetails(
        string label,
        Receiver receiver,
        Address address,
        AddressType addressType,
        string? building,
        string? apartment,
        string? floor,
        string? deliveryInstruction)
    {
        ValidateLabel(label);

        Label = label;
        Receiver = receiver;
        Address = address;
        AddressType = addressType;
        Building = building;
        Apartment = apartment;
        Floor = floor;
        DeliveryInstruction = deliveryInstruction;
        IsVerified = false;
    }

    public void UpdateGeoLocation(GeoLocation? geoLocation)
    {
        GeoLocation = geoLocation;
    }

    public void Verify()
    {
        IsVerified = true;
    }

    public void Unverify()
    {
        IsVerified = false;
    }

    public static bool IsValidLabel(string? label) => !string.IsNullOrWhiteSpace(label);

    private static void ValidateLabel(string label)
    {
        if (!IsValidLabel(label))
            throw ExceptionFactory.RequiredField("Address label cannot be empty.");
    }

    #endregion
}
