namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationTemplates;

public interface INotificationTemplateReadService
{
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<NotificationTemplate> Items, int TotalCount)> SearchAsync(
        NotificationChannelType? channel,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
