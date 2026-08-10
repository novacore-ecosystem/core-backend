namespace NovaCore.Notification.Persistence.Contexts.NotificationRules.Repositories;

public interface INotificationRuleRepository
{
    Task AddAsync(NotificationRule entity, CancellationToken ct = default);
}
