namespace NovaCore.BuildingBlock.Persistence.Outbox;

/// <summary>
/// Broker-agnostic, persistence-level outbox store interface.
/// Works with primitive types only (no Application/Domain/Contract dependency).
/// Persistence.Ef implements this generically; services layer on Application-level
/// IOutboxStore which translates typed events to primitive rows here.
/// </summary>
public interface IOutboxStore
{
    /// <summary>Enqueue a new outbox row with already-serialized payload and computed topic.</summary>
    Task EnqueueAsync(
        string eventType,
        string topic,
        string payload,
        string correlationId,
        CancellationToken ct = default);

    /// <summary>Fetch unprocessed rows, ordered by creation time.</summary>
    Task<IReadOnlyList<OutboxMessageSnapshot>> GetUnprocessedAsync(
        int batchSize,
        CancellationToken ct = default);

    /// <summary>Mark a row as successfully published.</summary>
    Task MarkProcessedAsync(Guid id, CancellationToken ct = default);

    /// <summary>Mark a row as failed and increment retry count.</summary>
    Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default);

    /// <summary>
    /// Delete one batch of processed rows whose ProcessedAt is older than <paramref name="olderThanUtc"/>.
    /// Never touches rows with ProcessedAt == null (pending/unpublished messages are never deleted).
    /// Returns the number of rows deleted, so the caller can loop until a batch comes back short.
    /// </summary>
    Task<int> DeleteProcessedBeforeAsync(
        DateTime olderThanUtc,
        int batchSize,
        CancellationToken ct = default);
}