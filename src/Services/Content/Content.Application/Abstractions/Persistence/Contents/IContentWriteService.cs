namespace NovaCore.Content.Application.Abstractions.Persistence.Contents;

public interface IContentWriteService
{
    /// <summary>Self-commits (bare SaveChangesAsync) - a brand-new Content is a single-aggregate write.</summary>
    Task CreateAsync(ContentEntity content, CancellationToken ct = default);

    /// <summary>
    /// Non-committing - the caller wraps this in unitOfWork.ExecuteTransactionAsync alongside the
    /// resulting ContentPublishedIntegrationEvent Outbox enqueue, so the state change and the
    /// event commit atomically.
    /// </summary>
    Task PublishAsync(Guid id, Guid versionId, DateTime publishedAt, CancellationToken ct = default);
}
