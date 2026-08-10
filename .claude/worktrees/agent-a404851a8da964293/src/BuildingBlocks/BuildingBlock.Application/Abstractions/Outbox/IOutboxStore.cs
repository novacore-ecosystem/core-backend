using NovaCore.BuildingBlock.Contract.Events;

namespace NovaCore.BuildingBlock.Application.Abstractions.Outbox;

/// <summary>
/// Write and read sides of the transactional outbox. EnqueueAsync is called by command
/// handlers, from inside the same unit of work as an aggregate change - it only tracks
/// the row (never calls SaveChanges), so it commits atomically with that change. The
/// other three methods are called by the relay, in its own separate scope/transaction.
/// </summary>
public interface IOutboxStore
{
    /// <summary>Tracks a new outbox row. Does not save - the caller's own SaveChanges commits it.</summary>
    Task EnqueueAsync<TEvent>(TEvent integrationEvent, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent;

    Task<IReadOnlyList<OutboxMessageSnapshot>> GetUnprocessedAsync(int batchSize, CancellationToken ct = default);

    Task MarkProcessedAsync(Guid id, CancellationToken ct = default);

    Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default);

    /// <summary>
    /// Delete one batch of processed rows older than <paramref name="olderThanUtc"/>. Never touches
    /// unprocessed rows. Returns the number deleted, so the caller (OutboxCleanupJob) can loop until
    /// a batch comes back short of batchSize.
    /// </summary>
    Task<int> DeleteProcessedBeforeAsync(DateTime olderThanUtc, int batchSize, CancellationToken ct = default);
}
