namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>A distribution channel (e.g. "Email", "Push", "SMS") a Campaign is enabled on - plain string, no ChannelType enum requested for this pass.</summary>
public sealed class CampaignChannel : BaseEntity<Guid>, IAuditable
{
    public Guid CampaignId { get; private set; }
    public string Channel { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }

    public Campaign Campaign { get; private set; } = default!;

    private CampaignChannel() { }

    /// <summary>Only Campaign may construct a CampaignChannel - see Campaign.AddChannel.</summary>
    internal static CampaignChannel Create(Guid campaignId, string channel)
    {
        if (string.IsNullOrWhiteSpace(channel))
            throw ExceptionFactory.RequiredField("Channel cannot be empty.");

        return new CampaignChannel
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaignId,
            Channel = channel,
            IsEnabled = true,
        };
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;
}
