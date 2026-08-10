namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationDispatches;

public interface INotificationDispatchReadService
{
    Task<NotificationDispatch?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<NotificationDispatch> Items, int TotalCount)> SearchAsync(
        DispatchStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Rows a worker should attempt next - Pending, or Failed with NextRetryAt due. Not exposed via API - consumed only by NovaCore.Notification.Infrastructure's dispatch worker.</summary>
    Task<IReadOnlyList<NotificationDispatch>> GetDueForProcessingAsync(int batchSize, CancellationToken ct = default);
}
