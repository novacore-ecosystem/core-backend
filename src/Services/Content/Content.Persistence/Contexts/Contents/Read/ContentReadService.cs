using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.Persistence.Ef.Criteria;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Application.Features.Contents.Search;
using NovaCore.Content.Persistence.Contexts.Contents.Repositories;
using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Contexts.Contents.Read;

public sealed class ContentReadService(IContentRepository contentRepo, ContentDbContext dbContext) : IContentReadService
{
    public async Task<ContentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await contentRepo.GetByIdAsync(
            id,
            query => query
                .Include(c => c.ContentType)
                .Include(c => c.Versions).ThenInclude(v => v.Localizations)
                .Include(c => c.Publications)
                .Include(c => c.WorkflowInstances)
                .Include(c => c.Relationships)
                .Include(c => c.TaxonomyAssignments).ThenInclude(a => a.Taxonomy)
                .Include(c => c.Audiences)
                .Include(c => c.Contributors),
            ct);
    }

    public async Task<ContentEntity?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Contents
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(c => c.ContentType)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<ContentEntity?> GetBySlugAsync(ContentSlug slug, CancellationToken ct = default)
    {
        return await dbContext.Contents
            .AsNoTracking()
            .Include(c => c.ContentType)
            .Include(c => c.PublishedVersion).ThenInclude(v => v!.Localizations)
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await contentRepo.ExistsByIdAsync(id, ct);
    }

    public async Task<bool> ExistsBySlugAsync(ContentSlug slug, CancellationToken ct = default)
    {
        return await dbContext.Contents.AsNoTracking().AnyAsync(c => c.Slug == slug, ct);
    }

    public async Task<IReadOnlyList<ContentAdminListItem>> SearchAdminAsync(
        CriteriaRequest criteria,
        DateTime? cursorCreatedAt,
        Guid? cursorId,
        int limit,
        string displayLanguage,
        CancellationToken ct = default)
    {
        IQueryable<ContentEntity> query = dbContext.Contents
            .AsNoTracking()
            .ApplyCriteria(ContentCriteriaDefinition.Instance, criteria)
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id);

        if (cursorCreatedAt.HasValue && cursorId.HasValue)
        {
            var createdAt = cursorCreatedAt.Value;
            var id = cursorId.Value;
            query = query.Where(c =>
                c.CreatedAt < createdAt || (c.CreatedAt == createdAt && c.Id.CompareTo(id) < 0));
        }

        var rows = await query
            .Select(c => new
            {
                c.Id,
                c.ContentTypeId,
                ContentTypeName = c.ContentType.Name,
                c.Slug,
                c.Status,
                c.Visibility,
                c.IsDeleted,
                c.CreatedAt,
                c.UpdatedAt,
                Title = c.CurrentVersion!.Localizations
                    .OrderBy(l => l.Culture.Value == displayLanguage ? 0 : 1)
                    .Select(l => l.Title)
                    .FirstOrDefault(),
            })
            .Take(limit + 1)
            .ToListAsync(ct);

        return [.. rows.Select(x => new ContentAdminListItem(
            x.Id, x.ContentTypeId, x.ContentTypeName, x.Slug.Value, x.Status, x.Visibility, x.IsDeleted,
            x.Title, x.CreatedAt, x.UpdatedAt))];
    }

    public async Task<IReadOnlyList<ContentLandingItem>> SearchLandingAsync(
        Guid? contentTypeId,
        string language,
        string fallbackLanguage,
        DateTime? cursorPublishedAt,
        Guid? cursorId,
        int limit,
        CancellationToken ct = default)
    {
        var query = dbContext.Contents
            .AsNoTracking()
            .Where(c => c.Status == ContentStatus.Published && c.PublishedVersionId != null);

        if (contentTypeId.HasValue)
            query = query.Where(c => c.ContentTypeId == contentTypeId.Value);

        var ordered = query
            .OrderByDescending(c => c.PublishedAt)
            .ThenByDescending(c => c.Id);

        IQueryable<ContentEntity> filtered = ordered;
        if (cursorPublishedAt.HasValue && cursorId.HasValue)
        {
            var publishedAt = cursorPublishedAt.Value;
            var id = cursorId.Value;
            filtered = ordered.Where(c =>
                c.PublishedAt < publishedAt || (c.PublishedAt == publishedAt && c.Id.CompareTo(id) < 0));
        }

        var rows = await filtered
            .Select(c => new
            {
                c.Id,
                c.ContentTypeId,
                c.Slug,
                c.PublishedAt,
                Localization = c.PublishedVersion!.Localizations
                    .OrderBy(l => l.Culture.Value == language ? 0 : l.Culture.Value == fallbackLanguage ? 1 : 2)
                    .Select(l => new { l.Culture, l.Title, l.Summary })
                    .FirstOrDefault(),
            })
            .Take(limit + 1)
            .ToListAsync(ct);

        return [.. rows
            .Where(x => x.Localization is not null)
            .Select(x => new ContentLandingItem(
                x.Id, x.ContentTypeId, x.Slug.Value, x.Localization!.Culture.Value, x.Localization.Title,
                x.Localization.Summary, x.PublishedAt!.Value))];
    }
}
