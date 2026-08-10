namespace NovaCore.Notification.Application.Abstractions.Persistence.UserNotifications;

public interface IUserNotificationReadService
{
    Task<UserNotification?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Cursor page of the caller's own notifications, newest first (CreatedAt desc, Id desc as
    /// tie-breaker). Pass the last-seen (createdAt, id) pair as the cursor to fetch the next page,
    /// or null for the first page. Fetches exactly <paramref name="limit"/> rows - callers wanting
    /// to know if there's a next page should request one extra and trim.
    /// </summary>
    Task<IReadOnlyList<UserNotification>> GetMineAsync(
        Guid userId,
        NotificationStatus? status,
        DateTime? cursorCreatedAt,
        Guid? cursorId,
        int limit,
        CancellationToken ct = default);
}
