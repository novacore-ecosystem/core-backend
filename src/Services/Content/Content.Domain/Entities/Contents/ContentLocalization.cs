namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// One culture's localization record for a Content, pointing at the ContentVersion carrying that
/// culture's translated payload. Kept as a proper join record (culture + version + its own
/// status) rather than per-language fields on Content/ContentVersion, so draft and published
/// localization state stay distinguishable per culture.
/// </summary>
public sealed class ContentLocalization : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid ContentId { get; private set; }
    public Content Content { get; private set; } = default!;
    public LanguageCode Culture { get; private set; } = null!;
    public Guid VersionId { get; private set; }
    public ContentVersion Version { get; private set; } = default!;
    public ContentStatus Status { get; private set; }

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private ContentLocalization() { }

    internal static ContentLocalization Create(
        Guid contentId,
        LanguageCode culture,
        Guid versionId,
        ContentStatus status = ContentStatus.Draft)
    {
        return new ContentLocalization
        {
            Id = Guid.CreateVersion7(),
            ContentId = contentId,
            Culture = culture,
            VersionId = versionId,
            Status = status,
        };
    }

    internal void ChangeVersion(Guid versionId)
    {
        VersionId = versionId;
    }

    internal void ChangeStatus(ContentStatus status)
    {
        Status = status;
    }
}
