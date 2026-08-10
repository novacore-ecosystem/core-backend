using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.NotificationDispatches.Repositories;

public sealed class NotificationDispatchRepo(NotificationMongoContext context) : INotificationDispatchRepository
{
    public async Task AddAsync(NotificationDispatch entity, CancellationToken ct = default)
    {
        await context.NotificationDispatches.InsertOneAsync(entity, cancellationToken: ct);
    }

    public async Task UpdateAsync(NotificationDispatch entity, CancellationToken ct = default)
    {
        await context.NotificationDispatches.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: ct);
    }
}
