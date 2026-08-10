namespace NovaCore.Notification.Persistence.Contexts.NotificationChannels.Repositories;

public interface INotificationChannelRepository
{
    Task UpdateAsync(NotificationChannel entity, CancellationToken ct = default);
}
