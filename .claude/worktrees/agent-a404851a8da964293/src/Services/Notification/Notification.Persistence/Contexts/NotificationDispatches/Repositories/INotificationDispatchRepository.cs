namespace NovaCore.Notification.Persistence.Contexts.NotificationDispatches.Repositories;

public interface INotificationDispatchRepository
{
    Task AddAsync(NotificationDispatch entity, CancellationToken ct = default);

    Task UpdateAsync(NotificationDispatch entity, CancellationToken ct = default);
}
