namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>A named targeting segment a Campaign runs against - structural reference only, no eligibility evaluation lives here.</summary>
public sealed class CampaignAudience : BaseEntity<Guid>, IAuditable
{
    public Guid CampaignId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }

    public Campaign Campaign { get; private set; } = default!;

    private CampaignAudience() { }

    /// <summary>Only Campaign may construct a CampaignAudience - see Campaign.AddAudience.</summary>
    internal static CampaignAudience Create(Guid campaignId, string name, string? description, int displayOrder)
    {
        ValidateName(name);

        return new CampaignAudience
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaignId,
            Name = name,
            Description = description,
            DisplayOrder = displayOrder,
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
            throw ExceptionFactory.RequiredField("Audience name cannot be empty.");
    }
}
