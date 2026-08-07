namespace NovaCore.Promotion.Domain.Entities.Campaigns;

/// <summary>A creative/reference file attached to a Campaign (banner image, brief document, ...). Storage location is an opaque Url - this service does not own file storage.</summary>
public sealed class CampaignAttachment : BaseEntity<Guid>, IAuditable
{
    public Guid CampaignId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string? ContentType { get; private set; }
    public long? SizeBytes { get; private set; }
    public int DisplayOrder { get; private set; }

    public Campaign Campaign { get; private set; } = default!;

    private CampaignAttachment() { }

    /// <summary>Only Campaign may construct a CampaignAttachment - see Campaign.AddAttachment.</summary>
    internal static CampaignAttachment Create(
        Guid campaignId,
        string fileName,
        string url,
        string? contentType,
        long? sizeBytes,
        int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw ExceptionFactory.RequiredField("Attachment file name cannot be empty.");

        if (string.IsNullOrWhiteSpace(url))
            throw ExceptionFactory.RequiredField("Attachment URL cannot be empty.");

        return new CampaignAttachment
        {
            Id = Guid.CreateVersion7(),
            CampaignId = campaignId,
            FileName = fileName,
            Url = url,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            DisplayOrder = displayOrder,
        };
    }
}
