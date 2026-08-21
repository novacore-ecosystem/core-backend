namespace NovaCore.Chat.Domain.Entities.Roles;

/// <summary>Conversation-level authorization role catalog, assignable by a conversation owner/admin - localized via ConversationRoleTranslation.</summary>
public sealed class ConversationRole : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ConversationRoleStatus Status { get; private set; }
    public bool IsSystem { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public ICollection<ConversationRoleTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private ConversationRole() { }

    public static ConversationRole Create(
        EntityCode code,
        string name,
        string? description = null,
        bool isSystem = false,
        ChatMetadata? metadata = null)
    {
        ValidateName(name);

        return new ConversationRole
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Status = ConversationRoleStatus.Active,
            IsSystem = isSystem,
            Metadata = metadata,
        };
    }
    #endregion

    #region Details & lifecycle
    public void UpdateDetails(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public void Activate() => Status = ConversationRoleStatus.Active;

    public void Deactivate() => Status = ConversationRoleStatus.Inactive;

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Role name cannot be empty.");
    }
    #endregion

    #region Translations
    public void Translate(LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        var existing = Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            existing.UpdateDetails(name, description);
            return;
        }

        Translations.Add(ConversationRoleTranslation.Create(Id, languageCode, name, description));
    }
    #endregion
}
