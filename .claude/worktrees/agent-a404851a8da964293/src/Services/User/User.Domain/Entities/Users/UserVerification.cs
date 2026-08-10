namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned child of User recording one verification attempt for a given VerificationType (email,
/// phone, identity, business, address). Kept as history rather than a single per-type row, so a
/// Rejected or Expired attempt does not erase the record of a prior successful one. Distinct
/// from UserContact.IsVerified: that flag is "is this specific contact value reachable" (OTP
/// confirmation on one Email/Phone row), while a VerificationType.Identity/Business/Address
/// record here has no corresponding UserContact at all - this is the broader KYC-style workflow.
/// </summary>
public sealed class UserVerification : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public VerificationType VerificationType { get; private set; }
    public VerificationStatus VerificationStatus { get; private set; } = VerificationStatus.Pending;
    public DateTime? VerifiedAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public string? Note { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserVerification() { }

    public static UserVerification Create(Guid userId, VerificationType verificationType, string? note = null)
    {
        return new UserVerification
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            VerificationType = verificationType,
            VerificationStatus = VerificationStatus.Pending,
            Note = note,
        };
    }

    // ============================================================================
    // Details & lifecycle
    // The Pending -> Verified/Rejected/Expired state transitions. Each transition
    // is only valid from Pending, so a decided record can't silently flip to a
    // different outcome.
    // ============================================================================

    #region Details & lifecycle

    public void Verify()
    {
        EnsurePending();

        VerificationStatus = VerificationStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
    }

    public void Reject(string? note = null)
    {
        EnsurePending();

        VerificationStatus = VerificationStatus.Rejected;
        Note = note ?? Note;
    }

    public void Expire()
    {
        EnsurePending();

        VerificationStatus = VerificationStatus.Expired;
        ExpiredAt = DateTime.UtcNow;
    }

    private void EnsurePending()
    {
        if (VerificationStatus != VerificationStatus.Pending)
            throw ExceptionFactory.InvalidState("Only a pending verification can transition state.");
    }

    #endregion
}
