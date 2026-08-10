using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.UserNotifications.Repositories;

public sealed class UserNotificationRepo(NotificationMongoContext context) : IUserNotificationRepository
{
    public async Task AddAsync(UserNotification entity, CancellationToken ct = default)
    {
        await context.UserNotifications.InsertOneAsync(entity, cancellationToken: ct);
    }

    public async Task UpdateAsync(UserNotification entity, CancellationToken ct = default)
    {
        await context.UserNotifications.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: ct);
    }
}
