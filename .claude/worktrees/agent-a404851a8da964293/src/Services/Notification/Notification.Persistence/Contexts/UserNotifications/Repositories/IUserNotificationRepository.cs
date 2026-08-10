namespace NovaCore.Notification.Persistence.Contexts.UserNotifications.Repositories;

public interface IUserNotificationRepository
{
    Task AddAsync(UserNotification entity, CancellationToken ct = default);

    Task UpdateAsync(UserNotification entity, CancellationToken ct = default);
}
