namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>Freeform backoffice label attached directly to a Campaign - no separate CampaignTagDefinition catalog was requested for this pass (unlike Order's OrderTagDefinition/OrderTag split).</summary>
public sealed class CampaignTag : BaseEntity<Guid>
{
    public Guid CampaignId { get; private set; }
    public string Label { get; private set; } = string.Empty;

    public Campaign Campaign { get; private set; } = default!;

    private CampaignTag() { }

    /// <summary>Only Campaign may construct a CampaignTag - see Campaign.AddTag.</summary>
    internal static CampaignTag Create(Guid campaignId, string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw ExceptionFactory.RequiredField("Tag label cannot be empty.");

        return new CampaignTag
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaignId,
            Label = label,
        };
    }
}
