using NovaCore.BuildingBlock.Domain.ValueObjects;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

namespace NovaCore.Auth.Domain.Entities.Invitations;

/// <summary>
/// Aggregate root - an admin-issued invitation for a new Account, pre-assigning the Role it
/// will receive on acceptance. Not owned by Account since the invitee has no Account yet.
/// </summary>
public sealed class Invitation : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Email Email { get; private set; } = null!;
    public Guid InvitedByAccountId { get; private set; }
    public Guid RoleId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public InvitationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private Invitation() { }

    public static Invitation Create(
        Email email,
        Guid invitedByAccountId,
        Guid roleId,
        string token,
        DateTime? expiresAt = null)
    {
        ValidateToken(token);

        return new Invitation
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            InvitedByAccountId = invitedByAccountId,
            RoleId = roleId,
            Token = token,
            Status = InvitationStatus.Pending,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.Add(InvitationDefaults.DefaultExpiry),
        };
    }

    #region Lifecycle

    public void Accept()
    {
        if (Status != InvitationStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Cannot accept a {Status} invitation.");

        if (ExpiresAt <= DateTime.UtcNow)
            throw ExceptionFactory.InvalidState("Cannot accept an expired invitation.");

        Status = InvitationStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status != InvitationStatus.Pending)
            return;

        Status = InvitationStatus.Expired;
    }

    public void Revoke()
    {
        if (Status != InvitationStatus.Pending)
            throw ExceptionFactory.InvalidStatus($"Cannot revoke a {Status} invitation.");

        Status = InvitationStatus.Revoked;
    }

    #endregion

    public static bool IsValidToken(string? token) => token.IsNotNullOrWhiteSpace();

    private static void ValidateToken(string token)
    {
        if (!IsValidToken(token))
            throw ExceptionFactory.RequiredField("Invitation token cannot be empty.");
    }
}
