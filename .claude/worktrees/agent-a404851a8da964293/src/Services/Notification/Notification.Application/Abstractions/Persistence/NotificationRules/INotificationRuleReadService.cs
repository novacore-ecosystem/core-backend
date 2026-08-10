namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationRules;

public interface INotificationRuleReadService
{
    Task<NotificationRule?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<NotificationRule> Items, int TotalCount)> SearchAsync(
        string? eventType,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
