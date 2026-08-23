using NovaCore.BuildingBlock.Domain.ValueObjects;

namespace NovaCore.Content.Application.Abstractions.Persistence.Contents;

public interface IContentWriteService
{
    /// <summary>
    /// Non-committing - the caller wraps this in unitOfWork.ExecuteTransactionAsync alongside the
    /// resulting ContentCreatedIntegrationEvent Outbox enqueue, so creation and the event commit
    /// atomically.
    /// </summary>
    Task CreateAsync(ContentEntity content, CancellationToken ct = default);

    /// <summary>
    /// Non-committing - the caller wraps this in unitOfWork.ExecuteTransactionAsync alongside the
    /// resulting ContentPublishedIntegrationEvent Outbox enqueue, so the state change and the
    /// event commit atomically.
    /// </summary>
    Task PublishAsync(Guid id, Guid versionId, DateTime publishedAt, CancellationToken ct = default);

    /// <summary>Creates a new draft version carrying <paramref name="language"/>'s content and
    /// returns its id/number. Non-committing.</summary>
    Task<(Guid Id, int VersionNumber)> CreateDraftVersionAsync(
        Guid contentId,
        LanguageCode language,
        string title,
        string summary,
        string body,
        Guid createdBy,
        ContentMetadata? metadata,
        CancellationToken ct = default);

    /// <summary>The single write path for draft editing and translation alike - upserts
    /// <paramref name="language"/>'s content onto <paramref name="versionId"/>. Non-committing.</summary>
    Task UpsertLocalizationAsync(
        Guid contentId,
        Guid versionId,
        LanguageCode language,
        string title,
        string summary,
        string body,
        Guid updatedBy,
        ContentMetadata? metadata,
        CancellationToken ct = default);

    /// <summary>Restores a prior version's full language set into a brand-new current version and
    /// returns its id/number. Non-committing.</summary>
    Task<(Guid Id, int VersionNumber)> RestoreVersionAsync(Guid contentId, Guid versionId, Guid restoredBy, CancellationToken ct = default);

    /// <summary>Soft-deletes the Content. Non-committing.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>Restores a soft-deleted Content. Non-committing - bypasses the soft-delete query
    /// filter to locate the row.</summary>
    Task RestoreAsync(Guid id, CancellationToken ct = default);

    /// <summary>Permanently removes a Content and its cascading children (versions, localizations,
    /// publications, ...). Used only by the hard-delete retention job. Non-committing.</summary>
    Task HardDeleteAsync(Guid id, CancellationToken ct = default);
}
