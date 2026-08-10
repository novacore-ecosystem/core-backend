using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.NotificationChannels.Repositories;

public sealed class NotificationChannelRepo(NotificationMongoContext context) : INotificationChannelRepository
{
    public async Task UpdateAsync(NotificationChannel entity, CancellationToken ct = default)
    {
        await context.NotificationChannels.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: ct);
    }
}
