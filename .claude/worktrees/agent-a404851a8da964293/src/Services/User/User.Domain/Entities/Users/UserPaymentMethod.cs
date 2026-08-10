namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned child of User representing a lightweight reference to a payment account owned by
/// Payment Service (src/Services/Payment) - PaymentAccountId points at that service's own
/// PaymentAccount.Id. Payment Service owns the actual account data (token, masked number,
/// holder name, expiration, issuer); UserService only keeps enough to list/display a user's
/// payment methods and track which one is default. See docs/services/payment-service.md and
/// docs/reference/payment-ownership-boundaries.md.
/// </summary>
public sealed class UserPaymentMethod : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public Guid PaymentAccountId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserPaymentMethod() { }

    public static UserPaymentMethod Create(
        Guid userId,
        Guid paymentAccountId,
        string displayName,
        bool isDefault = false)
    {
        ValidateDisplayName(displayName);

        return new UserPaymentMethod
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            PaymentAccountId = paymentAccountId,
            DisplayName = displayName,
            IsDefault = isDefault,
        };
    }

    // ============================================================================
    // Default flag
    // Manages the IsDefault toggle. The User aggregate root unmarks any previous
    // default before calling MarkAsDefault here, keeping the "at most one default
    // payment method" invariant centralized on User.
    // ============================================================================

    #region Default flag

    public void MarkAsDefault()
    {
        IsDefault = true;
    }

    public void UnmarkAsDefault()
    {
        IsDefault = false;
    }

    #endregion

    // ============================================================================
    // Details
    // Display name is the only mutable detail left on this side - everything else
    // (token, card details, verification) is Payment Service's own PaymentAccount.
    // ============================================================================

    #region Details

    public void Rename(string displayName)
    {
        ValidateDisplayName(displayName);

        DisplayName = displayName;
    }

    private static void ValidateDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw ExceptionFactory.RequiredField("Payment method display name cannot be empty.");
    }

    #endregion
}
