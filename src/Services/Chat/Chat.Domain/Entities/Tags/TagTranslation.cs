namespace NovaCore.Chat.Domain.Entities.Tags;

/// <summary>Per-language override of a Tag's Name/Description. Id doubles as the owning Tag's Id - composite key (Id, LanguageCode).</summary>
public sealed class TagTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Tag Tag { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private TagTranslation() { }

    /// <summary>Only Tag may construct a TagTranslation - see Tag.Translate.</summary>
    internal static TagTranslation Create(Guid tagId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new TagTranslation
        {
            Id = tagId,
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
            throw ExceptionFactory.RequiredField("Translated tag name cannot be empty.");
    }
}
