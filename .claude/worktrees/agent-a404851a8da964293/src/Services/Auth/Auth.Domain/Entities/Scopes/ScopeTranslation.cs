using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Auth.Domain.Entities.Scopes;

/// <summary>
/// Owned child of Scope - a locale-specific override of the scope's display copy. Standard
/// business translation (unlike TenantLocale's bootstrap data) - follows ProductTranslation's
/// shape exactly: Id doubles as the owning Scope's Id, one row per language, composite
/// (Id, LanguageCode) primary key (see ScopeTranslationConfig).
/// </summary>
public sealed class ScopeTranslation : BaseEntity<Guid>, IAuditable
{
    public Scope Scope { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private ScopeTranslation() { }

    internal static ScopeTranslation Create(
        Guid scopeId,
        LanguageCode languageCode,
        string name,
        string? description = null)
    {
        ValidateName(name);

        return new ScopeTranslation
        {
            Id = scopeId,
            LanguageCode = languageCode,
            Name = name,
            Description = description,
        };
    }

    public void UpdateContent(string name, string? description)
    {
        ValidateName(name);

        Name = name;
        Description = description;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Translated scope name cannot be empty.");
    }
}
