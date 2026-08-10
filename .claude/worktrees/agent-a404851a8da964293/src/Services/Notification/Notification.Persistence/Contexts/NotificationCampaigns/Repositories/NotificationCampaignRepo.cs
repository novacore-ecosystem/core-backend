using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.NotificationCampaigns.Repositories;

public sealed class NotificationCampaignRepo(NotificationMongoContext context) : INotificationCampaignRepository
{
    public async Task AddAsync(NotificationCampaign entity, CancellationToken ct = default)
    {
        await context.NotificationCampaigns.InsertOneAsync(entity, cancellationToken: ct);
    }
}
