namespace NovaCore.Chat.Domain.Entities.Permissions;

/// <summary>Per-language override of a ConversationPermission's Name/Description. Id doubles as the owning permission's Id - composite key (Id, LanguageCode).</summary>
public sealed class ConversationPermissionTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public ConversationPermission Permission { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationPermissionTranslation() { }

    /// <summary>Only ConversationPermission may construct a ConversationPermissionTranslation - see ConversationPermission.Translate.</summary>
    internal static ConversationPermissionTranslation Create(Guid permissionId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new ConversationPermissionTranslation
        {
            Id = permissionId,
            LanguageCode = languageCode,
            Name = name,
            Description = description,
        };
    }

    internal void UpdateDetails(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Translated permission name cannot be empty.");
    }
}
