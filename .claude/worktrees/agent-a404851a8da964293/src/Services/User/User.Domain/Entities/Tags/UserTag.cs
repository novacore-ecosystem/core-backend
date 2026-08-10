namespace NovaCore.User.Domain.Entities.Tags;

/// <summary>
/// Independent, reusable segmentation lookup - flat, no hierarchy. Not a membership level or
/// loyalty tier; a free-form label ("VIP_CUSTOMER", "WHOLESALE", "POTENTIAL_FRAUD") used for
/// business rules and targeting. Admin-managed, so it supports localized display text via
/// Translations - Name itself is the internal, language-independent code and is never
/// translated. Users reference tags via UserTagMapping.
/// </summary>
public sealed class UserTag : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public TagCode Name { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public string? Color { get; private set; }
    public TagScope Scope { get; private set; }
    public bool IsSystem { get; private set; }
    public ICollection<UserTagTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserTag() { }

    public static UserTag Create(
        Guid id,
        TagCode name,
        string description,
        TagScope scope,
        string? color = null,
        bool isSystem = false)
    {
        return new UserTag
        {
            Id = id,
            Name = name,
            Description = description,
            Scope = scope,
            Color = color,
            IsSystem = isSystem,
        };
    }

    // ============================================================================
    // Translations
    // Manages the per-language DisplayName/Description override for this tag,
    // upserting by language code. The internal Name code is never part of a
    // translation.
    // ============================================================================

    #region Translations

    public void Translate(LanguageCode languageCode, string displayName, string? description = null)
    {
        var existingTranslation = Translations
            .FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existingTranslation != null)
        {
            existingTranslation.UpdateDetails(displayName, description);
            return;
        }

        var translation = UserTagTranslation.Create(
            Id,
            languageCode,
            displayName,
            description);
        Translations.Add(translation);
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // Core descriptive fields and scope. System tags (IsSystem) are predefined
    // by the platform (e.g. "BLACKLISTED", "POTENTIAL_FRAUD") and cannot be
    // renamed or rescoped, only have their description/color updated.
    // ============================================================================

    #region Details & lifecycle

    public void Rename(TagCode name)
    {
        EnsureNotSystem();

        Name = name;
    }

    public void UpdateDetails(string description, string? color)
    {
        Description = description;
        Color = color;
    }

    public void ChangeScope(TagScope scope)
    {
        EnsureNotSystem();

        Scope = scope;
    }

    private void EnsureNotSystem()
    {
        if (IsSystem)
            throw ExceptionFactory.InvalidState("System tags cannot be renamed or rescoped.");
    }

    #endregion
}
