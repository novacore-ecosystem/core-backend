namespace NovaCore.Chat.Domain.Entities.Contacts;

/// <summary>
/// Business identity a conversation participant is reached through - a guest (UserId is null,
/// identified only by the contact details they submitted) or an authenticated user (UserId set).
/// Never assumes every conversation participant is already a User - see spec section 39/40.
/// </summary>
public sealed class Contact : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public Guid? UserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public Email? Email { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Contact() { }

    public static Contact Create(
        string displayName,
        Guid? userId = null,
        Email? email = null,
        PhoneNumber? phone = null,
        ChatMetadata? metadata = null)
    {
        ValidateDisplayName(displayName);

        return new Contact
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            DisplayName = displayName,
            Email = email,
            Phone = phone,
            Metadata = metadata,
        };
    }
    #endregion

    #region Details & lifecycle
    public void UpdateDetails(string displayName, Email? email, PhoneNumber? phone)
    {
        ValidateDisplayName(displayName);

        DisplayName = displayName;
        Email = email;
        Phone = phone;
    }

    /// <summary>Links a previously-guest Contact to an authenticated User once one is resolved for it - never overwrites an existing link.</summary>
    public void LinkUser(Guid userId)
    {
        if (UserId is not null)
            throw ExceptionFactory.InvalidState("Contact is already linked to a user.");

        UserId = userId;
    }

    public void UpdateMetadata(ChatMetadata metadata)
    {
        Metadata = metadata;
    }

    public static bool IsValidDisplayName(string? displayName) => !string.IsNullOrWhiteSpace(displayName);

    private static void ValidateDisplayName(string displayName)
    {
        if (!IsValidDisplayName(displayName))
            throw ExceptionFactory.RequiredField("Contact display name cannot be empty.");
    }
    #endregion
}
