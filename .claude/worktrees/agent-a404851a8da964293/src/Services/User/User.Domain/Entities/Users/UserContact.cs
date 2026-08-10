namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned child of User representing one reachable contact channel (email, phone, messaging app).
/// "Primary" is scoped per ContactType - a user can have one primary Email and, independently,
/// one primary Phone - enforced by the User aggregate root.
/// </summary>
public sealed class UserContact : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public ContactType ContactType { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string? Label { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsVerified { get; private set; }
    public DateTime? VerifiedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserContact() { }

    public static UserContact Create(
        Guid userId,
        ContactType contactType,
        string value,
        string? label = null,
        bool isPrimary = false)
    {
        ValidateValue(value);

        return new UserContact
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            ContactType = contactType,
            Value = value,
            Label = label,
            IsPrimary = isPrimary,
            IsVerified = false,
            VerifiedAt = null,
        };
    }

    // ============================================================================
    // Primary flag
    // Manages the IsPrimary toggle. The User aggregate root unmarks any previous
    // primary contact of the same ContactType before calling MarkAsPrimary here,
    // keeping the "at most one primary per ContactType" invariant on User.
    // ============================================================================

    #region Primary flag

    public void MarkAsPrimary()
    {
        IsPrimary = true;
    }

    public void UnmarkAsPrimary()
    {
        IsPrimary = false;
    }

    #endregion

    // ============================================================================
    // Verification
    // Tracks whether this contact channel has been confirmed reachable (e.g. via
    // an OTP or confirmation link) and when.
    // ============================================================================

    #region Verification

    public void Verify()
    {
        IsVerified = true;
        VerifiedAt = DateTime.UtcNow;
    }

    public void Unverify()
    {
        IsVerified = false;
        VerifiedAt = null;
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Value/label updates and the shared value-validation rule. Changing Value
    // resets verification, since the previously verified value no longer applies.
    // ============================================================================

    #region Details & lifecycle

    public void UpdateValue(string value)
    {
        ValidateValue(value);

        Value = value;
        Unverify();
    }

    public void UpdateLabel(string? label)
    {
        Label = label;
    }

    public static bool IsValidValue(string? value) => !string.IsNullOrWhiteSpace(value);

    private static void ValidateValue(string value)
    {
        if (!IsValidValue(value))
            throw ExceptionFactory.RequiredField("Contact value cannot be empty.");
    }

    #endregion
}
