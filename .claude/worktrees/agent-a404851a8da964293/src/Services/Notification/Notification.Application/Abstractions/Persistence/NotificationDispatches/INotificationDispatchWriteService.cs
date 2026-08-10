namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationDispatches;

public interface INotificationDispatchWriteService
{
    /// <summary>
    /// Non-committing: CreateNotificationDispatchHandler calls this once per requested channel in
    /// a loop, then commits once itself afterward (IUnitOfWork.SaveChangesAsync) - same batching
    /// shape as Auth's RefreshToken sync (Correction 2 in the persistence refactor tracker).
    /// </summary>
    Task CreateAsync(NotificationDispatch entity, CancellationToken ct = default);

    /// <summary>Persists an already-mutated entity (Mongo ReplaceOneAsync) - the caller (NotificationDispatchWorker) applies MarkProcessing/MarkSent/MarkFailed before calling this.</summary>
    Task UpdateAsync(NotificationDispatch entity, CancellationToken ct = default);
}
