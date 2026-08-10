namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationCampaigns;

public interface INotificationCampaignWriteService
{
    Task CreateAsync(NotificationCampaign entity, CancellationToken ct = default);
}
