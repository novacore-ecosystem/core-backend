namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationCampaigns;

public interface INotificationCampaignReadService
{
    Task<NotificationCampaign?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<NotificationCampaign> Items, int TotalCount)> SearchAsync(
        CampaignStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
