namespace NovaCore.Chat.Domain.Entities.Tags;

/// <summary>Server-configured conversation label catalog - localized via TagTranslation.</summary>
public sealed class Tag : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TagStatus Status { get; private set; }
    public int SortOrder { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public ICollection<TagTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Tag() { }

    public static Tag Create(
        EntityCode code,
        string name,
        string? description = null,
        int sortOrder = 0,
        ChatMetadata? metadata = null)
    {
        ValidateName(name);

        return new Tag
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            Description = description,
            Status = TagStatus.Active,
            SortOrder = sortOrder,
            Metadata = metadata,
        };
    }
    #endregion

    #region Details & lifecycle
    public void UpdateDetails(string name, string? description, int sortOrder)
    {
        ValidateName(name);

        Name = name;
        Description = description;
        SortOrder = sortOrder;
    }

    public void Activate() => Status = TagStatus.Active;

    public void Deactivate() => Status = TagStatus.Inactive;

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Tag name cannot be empty.");
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

        Translations.Add(TagTranslation.Create(Id, languageCode, name, description));
    }
    #endregion
}
