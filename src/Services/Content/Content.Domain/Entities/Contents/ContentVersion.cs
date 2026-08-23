namespace NovaCore.Content.Domain.Entities.Contents;

/// <summary>
/// One immutable-once-published edition of a Content: a version number grouping every language's
/// localized payload together (see <see cref="ContentLocalization"/>, which owns the actual
/// title/body per culture). A Content may have many versions; the currently-edited draft and the
/// currently-published version are tracked independently on Content (CurrentVersionId /
/// PublishedVersionId) and are not necessarily the same row.
/// </summary>
public sealed class ContentVersion : BaseEntity<Guid>, IAuditable, ITenantEntity
{
    public Guid ContentId { get; private set; }
    public Content Content { get; private set; } = default!;
    public int VersionNumber { get; private set; }
    public ContentStatus Status { get; private set; }
    public Guid CreatedBy { get; private set; }

    public ICollection<ContentLocalization> Localizations { get; private set; } = [];

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
        Guid createdBy,
        ContentStatus status = ContentStatus.Draft)
    {
        return new ContentVersion
        {
            Id = Guid.CreateVersion7(),
            ContentId = contentId,
            VersionNumber = versionNumber,
            Status = status,
            CreatedBy = createdBy,
        };
    }

    internal void ChangeStatus(ContentStatus status)
    {
        Status = status;
    }

    public ContentLocalization? GetLocalization(LanguageCode culture)
        => Localizations.FirstOrDefault(l => l.Culture == culture);
}
