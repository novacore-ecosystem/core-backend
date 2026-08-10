namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationGroups;

public interface INotificationGroupReadService
{
    Task<NotificationGroup?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<NotificationGroup> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
