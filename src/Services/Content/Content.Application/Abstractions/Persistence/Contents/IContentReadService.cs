using NovaCore.BuildingBlock.Criteria.Requests;

namespace NovaCore.Content.Application.Abstractions.Persistence.Contents;

public interface IContentReadService
{
    Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Bypasses the soft-delete query filter - the only supported way to load a
    /// soft-deleted Content, used by Restore and by the hard-delete eligibility scan.</summary>
    Task<ContentEntity?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default);

    Task<ContentEntity?> GetBySlugAsync(ContentSlug slug, CancellationToken ct = default);

    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsBySlugAsync(ContentSlug slug, CancellationToken ct = default);

    /// <summary>Fetches up to <paramref name="limit"/> + 1 admin list rows (the extra row lets the
    /// caller detect "has more" without a second round trip), filtered by
    /// <see cref="Features.Contents.Search.ContentCriteriaDefinition"/> and keyset-paginated on a
    /// fixed (CreatedAt desc, Id desc) order regardless of any Sorts on <paramref name="criteria"/>.</summary>
    Task<IReadOnlyList<ContentAdminListItem>> SearchAdminAsync(
        CriteriaRequest criteria,
        DateTime? cursorCreatedAt,
        Guid? cursorId,
        int limit,
        string displayLanguage,
        CancellationToken ct = default);

    /// <summary>Fetches up to <paramref name="limit"/> + 1 published, non-deleted landing rows,
    /// optionally filtered by content type, resolved to <paramref name="language"/> (falling back to
    /// <paramref name="fallbackLanguage"/>, then to whatever language the version does carry) and
    /// keyset-paginated on a fixed (PublishedAt desc, Id desc) order.</summary>
    Task<IReadOnlyList<ContentLandingItem>> SearchLandingAsync(
        Guid? contentTypeId,
        string language,
        string fallbackLanguage,
        DateTime? cursorPublishedAt,
        Guid? cursorId,
        int limit,
        CancellationToken ct = default);
}
