namespace NovaCore.Chat.Domain.Entities.Stickers;

/// <summary>Server-configured sticker catalog entry - localized via StickerTranslation (spec section 37).</summary>
public sealed class Sticker : AggregateRoot<Guid>, IAuditable, ITenantEntity
{
    public EntityCode Code { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string AssetKey { get; private set; } = string.Empty;
    public StickerStatus Status { get; private set; }
    public int SortOrder { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public ICollection<StickerTranslation> Translations { get; private set; } = [];

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    #region Constructor
    private Sticker() { }

    public static Sticker Create(
        EntityCode code,
        string name,
        string assetKey,
        int sortOrder = 0,
        ChatMetadata? metadata = null)
    {
        ValidateName(name);

        if (string.IsNullOrWhiteSpace(assetKey))
            throw ExceptionFactory.RequiredField("Sticker asset key cannot be empty.");

        return new Sticker
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            AssetKey = assetKey,
            Status = StickerStatus.Active,
            SortOrder = sortOrder,
            Metadata = metadata,
        };
    }
    #endregion

    #region Details & lifecycle
    public void UpdateDetails(string name, string assetKey, int sortOrder)
    {
        ValidateName(name);

        Name = name;
        AssetKey = assetKey;
        SortOrder = sortOrder;
    }

    public void Activate() => Status = StickerStatus.Active;

    public void Deactivate() => Status = StickerStatus.Inactive;

    public static bool IsValidName(string? name) => !string.IsNullOrWhiteSpace(name);

    private static void ValidateName(string name)
    {
        if (!IsValidName(name))
            throw ExceptionFactory.RequiredField("Sticker name cannot be empty.");
    }
    #endregion

    #region Translations
    public void Translate(LanguageCode languageCode, string name)
    {
        ValidateName(name);

        var existing = Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            existing.UpdateName(name);
            return;
        }

        Translations.Add(StickerTranslation.Create(Id, languageCode, name));
    }
    #endregion
}
