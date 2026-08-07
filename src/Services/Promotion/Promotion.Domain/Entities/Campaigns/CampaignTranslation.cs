namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>
/// Per-language override of a Campaign's Name/Description. Identity is CampaignId + LanguageCode
/// (Phase 3.1 correction) - no surrogate Id, since a Translation's identity is fully determined by
/// its parent and language, not an independent lifecycle. Renamed from CampaignLocalization during
/// the Phase 2.5 Domain Standardization Review for naming consistency with every other Translation
/// entity in the platform.
/// </summary>
public sealed class CampaignTranslation : BaseEntity, IAuditable, ITenantEntity
{
    public Guid CampaignId { get; private set; }
    public LanguageCode LanguageCode { get; private set; } = default!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Campaign Campaign { get; private set; } = default!;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private CampaignTranslation() { }

    /// <summary>Only Campaign may construct a CampaignTranslation - see Campaign.Translate.</summary>
    internal static CampaignTranslation Create(Guid campaignId, LanguageCode languageCode, string name, string? description)
    {
        ValidateName(name);

        return new CampaignTranslation
        {
            CampaignId = campaignId,
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
            throw ExceptionFactory.RequiredField("Translated campaign name cannot be empty.");
    }
}
