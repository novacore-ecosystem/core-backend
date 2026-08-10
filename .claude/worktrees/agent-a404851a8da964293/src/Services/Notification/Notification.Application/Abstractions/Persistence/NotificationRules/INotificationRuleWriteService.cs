namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationRules;

public interface INotificationRuleWriteService
{
    Task CreateAsync(NotificationRule entity, CancellationToken ct = default);
}
