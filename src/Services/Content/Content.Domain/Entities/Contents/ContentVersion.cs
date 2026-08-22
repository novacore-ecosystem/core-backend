namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// One immutable-once-published snapshot of a Content's editorial payload. A Content may have
/// many versions; the currently-edited draft and the currently-published version are tracked
/// independently on Content (CurrentVersionId / PublishedVersionId) and are not necessarily
/// the same row.
/// </summary>
public sealed class ContentVersion : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid ContentId { get; private set; }
    public Content Content { get; private set; } = default!;
    public int VersionNumber { get; private set; }
    public ContentStatus Status { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public ContentMetadata Metadata { get; private set; } = new();
    public Guid CreatedBy { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentVersion() { }

    internal static ContentVersion Create(
        Guid contentId,
        int versionNumber,
        string title,
        string summary,
        string body,
        Guid createdBy,
        ContentMetadata? metadata = null,
        ContentStatus status = ContentStatus.Draft)
    {
        ValidateTitle(title);

        return new ContentVersion
        {
            Id = Guid.CreateVersion7(),
            ContentId = contentId,
            VersionNumber = versionNumber,
            Status = status,
            Title = title,
            Summary = summary,
            Body = body,
            Metadata = metadata ?? new ContentMetadata(),
            CreatedBy = createdBy,
        };
    }

    internal void UpdateContent(string title, string summary, string body, ContentMetadata? metadata)
    {
        if (Status is ContentStatus.Published or ContentStatus.Archived)
            throw ExceptionFactory.InvalidStatus("Cannot edit a version that has already been published or archived - create a new version instead.");

        ValidateTitle(title);

        Title = title;
        Summary = summary;
        Body = body;
        Metadata = metadata ?? Metadata;
    }

    internal void ChangeStatus(ContentStatus status)
    {
        Status = status;
    }

    public static bool IsValidTitle(string? title) => !string.IsNullOrWhiteSpace(title);

    private static void ValidateTitle(string title)
    {
        if (!IsValidTitle(title))
            throw ExceptionFactory.RequiredField("Content version title cannot be empty.");
    }
}
