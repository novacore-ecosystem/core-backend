namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationTemplates;

public interface INotificationTemplateWriteService
{
    Task CreateAsync(NotificationTemplate entity, CancellationToken ct = default);
}
