namespace NovaCore.Chat.Domain.Entities.Messages;

/// <summary>Metadata/reference to an attached file - never stores binary content, only a pointer into the storage system (spec section 23).</summary>
public sealed class MessageAttachment : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid MessageId { get; private set; }
    public Message Message { get; private set; } = default!;
    public AttachmentType Type { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Size { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public ChatMetadata? Metadata { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private MessageAttachment() { }

    /// <summary>Only Message may construct a MessageAttachment - see Message.AddAttachment.</summary>
    internal static MessageAttachment Create(
        Guid messageId,
        AttachmentType type,
        string fileName,
        string contentType,
        long size,
        string storageKey,
        int? width,
        int? height,
        TimeSpan? duration,
        ChatMetadata? metadata)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            throw ExceptionFactory.RequiredField("Attachment storage key cannot be empty.");

        return new MessageAttachment
        {
            Id = Guid.CreateVersion7(),
            MessageId = messageId,
            Type = type,
            FileName = fileName,
            ContentType = contentType,
            Size = size,
            StorageKey = storageKey,
            Width = width,
            Height = height,
            Duration = duration,
            Metadata = metadata,
        };
    }
}
