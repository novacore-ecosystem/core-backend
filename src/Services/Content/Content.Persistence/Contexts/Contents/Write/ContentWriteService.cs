using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Content.Application.Abstractions.Persistence.Contents;
using NovaCore.Content.Persistence.Contexts.Contents.Repositories;
using NovaCore.Content.Persistence.Engine;

namespace NovaCore.Content.Persistence.Contexts.Contents.Write;

public sealed class ContentWriteService(IContentRepository contentRepo, ContentDbContext dbContext) : IContentWriteService
{
    // Every mutation that reaches into Versions/Localizations must load them first - the generic
    // repository's id-only UpdateAsync overload queries without includes, which would otherwise
    // hand the aggregate method an empty Versions collection and make it fail as "not found".
    private static readonly Func<IQueryable<ContentEntity>, IQueryable<ContentEntity>> WithVersionsAndLocalizations =
        query => query.Include(c => c.Versions).ThenInclude(v => v.Localizations);

    public async Task CreateAsync(ContentEntity content, CancellationToken ct = default)
    {
        await contentRepo.AddAsync(content, ct);
        // no commit here - the calling handler wraps this in its own ExecuteTransactionAsync
        // alongside the ContentCreatedIntegrationEvent Outbox enqueue.
    }

    public async Task PublishAsync(Guid id, Guid versionId, DateTime publishedAt, CancellationToken ct = default)
    {
        await contentRepo.UpdateAsync(id, WithVersionsAndLocalizations, c => c.Publish(versionId, publishedAt), ct);
        // no commit here - the calling handler wraps this in its own ExecuteTransactionAsync
        // alongside the ContentPublishedIntegrationEvent Outbox enqueue.
    }

    public async Task<(Guid Id, int VersionNumber)> CreateDraftVersionAsync(
        Guid contentId,
        LanguageCode language,
        string title,
        string summary,
        string body,
        Guid createdBy,
        ContentMetadata? metadata,
        CancellationToken ct = default)
    {
        var result = (Id: Guid.Empty, VersionNumber: 0);
        await contentRepo.UpdateAsync(contentId, WithVersionsAndLocalizations, c =>
        {
            var version = c.CreateDraftVersion(language, title, summary, body, createdBy, metadata);
            result = (version.Id, version.VersionNumber);
        }, ct);

        return result;
    }

    public async Task UpsertLocalizationAsync(
        Guid contentId,
        Guid versionId,
        LanguageCode language,
        string title,
        string summary,
        string body,
        Guid updatedBy,
        ContentMetadata? metadata,
        CancellationToken ct = default)
    {
        await contentRepo.UpdateAsync(contentId, WithVersionsAndLocalizations, c =>
            c.UpsertLocalization(versionId, language, title, summary, body, updatedBy, metadata), ct);
    }

    public async Task<(Guid Id, int VersionNumber)> RestoreVersionAsync(Guid contentId, Guid versionId, Guid restoredBy, CancellationToken ct = default)
    {
        var result = (Id: Guid.Empty, VersionNumber: 0);
        await contentRepo.UpdateAsync(contentId, WithVersionsAndLocalizations, c =>
        {
            var restored = c.RestoreVersion(versionId, restoredBy);
            result = (restored.Id, restored.VersionNumber);
        }, ct);

        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await contentRepo.UpdateAsync(id, c => c.Delete(), ct);
    }

    public async Task RestoreAsync(Guid id, CancellationToken ct = default)
    {
        var content = await dbContext.Contents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException(nameof(ContentEntity), id);

        content.Restore();
    }

    public async Task HardDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var content = await dbContext.Contents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (content is null)
            return;

        dbContext.Contents.Remove(content);
    }
}
