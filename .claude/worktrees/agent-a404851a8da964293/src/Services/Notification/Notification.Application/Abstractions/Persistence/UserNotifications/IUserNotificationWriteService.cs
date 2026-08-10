namespace NovaCore.Notification.Application.Abstractions.Persistence.UserNotifications;

public interface IUserNotificationWriteService
{
    Task CreateAsync(UserNotification entity, CancellationToken ct = default);

    /// <summary>Persists an already-mutated entity (Mongo ReplaceOneAsync) - the caller applies domain methods (e.g. MarkAsRead) before calling this.</summary>
    Task UpdateAsync(UserNotification entity, CancellationToken ct = default);
}
