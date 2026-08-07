namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>An additional scheduled window for a Campaign, beyond its own StartTime/EndTime (e.g. recurring or event-specific runs).</summary>
public sealed class CampaignSchedule : BaseEntity<Guid>, IAuditable
{
    public Guid CampaignId { get; private set; }
    public Period Period { get; private set; } = default!;
    public string? Label { get; private set; }
    public int DisplayOrder { get; private set; }

    public Campaign Campaign { get; private set; } = default!;

    private CampaignSchedule() { }

    /// <summary>Only Campaign may construct a CampaignSchedule - see Campaign.AddSchedule.</summary>
    internal static CampaignSchedule Create(Guid campaignId, Period period, string? label, int displayOrder)
    {
        return new CampaignSchedule
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaignId,
            Period = period,
            Label = label,
            DisplayOrder = displayOrder,
        };
    }

    internal void UpdateDetails(Period period, string? label)
    {
        Period = period;
        Label = label;
    }
}
