namespace NovaCore.Notification.Persistence.Contexts.NotificationCampaigns.Repositories;

public interface INotificationCampaignRepository
{
    Task AddAsync(NotificationCampaign entity, CancellationToken ct = default);
}
