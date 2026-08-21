namespace NovaCore.Chat.Domain.Entities.Roles;

/// <summary>Per-language override of a ConversationRole's Name/Description. Id doubles as the owning role's Id - composite key (Id, LanguageCode).</summary>
public sealed class ConversationRoleTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public ConversationRole Role { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ConversationRoleTranslation() { }

    /// <summary>Only ConversationRole may construct a ConversationRoleTranslation - see ConversationRole.Translate.</summary>
    internal static ConversationRoleTranslation Create(Guid roleId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new ConversationRoleTranslation
        {
            Id = roleId,
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
            throw ExceptionFactory.RequiredField("Translated role name cannot be empty.");
    }
}
