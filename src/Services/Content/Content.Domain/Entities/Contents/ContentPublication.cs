namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// One publication-lifecycle record for a Content, always tied to a specific ContentVersion.
/// Kept independent from ordinary editing: publishing/scheduling/unpublishing never touches
/// draft data on ContentVersion, and history is preserved by never deleting past publications.
/// </summary>
public sealed class ContentPublication : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid ContentId { get; private set; }
    public Content Content { get; private set; } = default!;
    public Guid VersionId { get; private set; }
    public ContentVersion Version { get; private set; } = default!;
    public PublicationStatus Status { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? UnpublishedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentPublication() { }

    internal static ContentPublication Create(
        Guid contentId,
        Guid versionId,
        DateTime? scheduledAt = null,
        DateTime? expiresAt = null)
    {
        return new ContentPublication
        {
            Id = Guid.CreateVersion7(),
            ContentId = contentId,
            VersionId = versionId,
            Status = scheduledAt.HasValue ? PublicationStatus.Scheduled : PublicationStatus.Unpublished,
            ScheduledAt = scheduledAt,
            ExpiresAt = expiresAt,
        };
    }

    internal void MarkPublished(DateTime publishedAt)
    {
        if (Status is PublicationStatus.Cancelled or PublicationStatus.Expired)
            throw ExceptionFactory.InvalidStatus("Cannot publish a cancelled or expired publication record.");

        Status = PublicationStatus.Published;
        PublishedAt = publishedAt;
    }

    internal void MarkUnpublished(DateTime unpublishedAt)
    {
        Status = PublicationStatus.Unpublished;
        UnpublishedAt = unpublishedAt;
    }

    internal void MarkExpired()
    {
        Status = PublicationStatus.Expired;
    }

    internal void Cancel()
    {
        if (Status == PublicationStatus.Published)
            throw ExceptionFactory.InvalidStatus("Cannot cancel a publication that has already gone live - unpublish it instead.");

        Status = PublicationStatus.Cancelled;
    }
}
