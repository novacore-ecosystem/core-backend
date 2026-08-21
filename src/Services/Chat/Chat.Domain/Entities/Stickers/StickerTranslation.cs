namespace NovaCore.Chat.Domain.Entities.Stickers;

/// <summary>Per-language override of a Sticker's Name. Id doubles as the owning Sticker's Id - composite key (Id, LanguageCode).</summary>
public sealed class StickerTranslation : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Sticker Sticker { get; private set; } = default!;
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private StickerTranslation() { }

    /// <summary>Only Sticker may construct a StickerTranslation - see Sticker.Translate.</summary>
    internal static StickerTranslation Create(Guid stickerId, LanguageCode languageCode, string name)
    {
        ValidateName(name);

        return new StickerTranslation
        {
            Id = stickerId,
            LanguageCode = languageCode,
            Name = name,
        };
    }

    internal void UpdateName(string name)
    {
        ValidateName(name);

        Name = name;
    }

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Translated sticker name cannot be empty.");
    }
}
