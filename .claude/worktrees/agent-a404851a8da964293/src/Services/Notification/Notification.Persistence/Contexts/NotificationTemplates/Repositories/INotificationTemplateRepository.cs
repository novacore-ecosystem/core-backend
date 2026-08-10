namespace NovaCore.Notification.Persistence.Contexts.NotificationTemplates.Repositories;

public interface INotificationTemplateRepository
{
    Task AddAsync(NotificationTemplate entity, CancellationToken ct = default);
}
