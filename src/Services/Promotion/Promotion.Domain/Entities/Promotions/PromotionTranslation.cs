namespace NovaCore.Promotion.Domain.Entities.Promotions;

/// <summary>Per-language override of a Promotion's Name/Description. Identity is PromotionId + LanguageCode (Phase 3.1 correction) - no surrogate Id.</summary>
public sealed class PromotionTranslation : BaseEntity, IAuditable, ITenantEntity
{
    public Guid PromotionId { get; private set; }
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public PromotionEntity Promotion { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private PromotionTranslation() { }

    /// <summary>Only Promotion may construct a PromotionTranslation - see Promotion.Translate.</summary>
    internal static PromotionTranslation Create(Guid promotionId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new PromotionTranslation
        {
            PromotionId = promotionId,
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

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw ExceptionFactory.RequiredField("Translated promotion name cannot be empty.");
    }
}
