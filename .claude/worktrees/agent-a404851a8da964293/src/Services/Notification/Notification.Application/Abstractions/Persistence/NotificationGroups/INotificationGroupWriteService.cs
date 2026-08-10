namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationGroups;

public interface INotificationGroupWriteService
{
    Task CreateAsync(NotificationGroup entity, CancellationToken ct = default);
}
