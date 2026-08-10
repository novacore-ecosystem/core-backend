using NovaCore.Notification.Application.Abstractions.Persistence.UserNotifications;
using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.UserNotifications.Read;

public sealed class UserNotificationReadService(NotificationMongoContext context) : IUserNotificationReadService
{
    public async Task<UserNotification?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.UserNotifications.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<UserNotification>> GetMineAsync(
        Guid userId,
        NotificationStatus? status,
        DateTime? cursorCreatedAt,
        Guid? cursorId,
        int limit,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<UserNotification>.Filter;
        var filter = filterBuilder.Eq(x => x.UserId, userId);

        if (status is not null)
            filter &= filterBuilder.Eq(x => x.Status, status.Value);

        if (cursorCreatedAt is not null && cursorId is not null)
        {
            filter &= filterBuilder.Or(
                filterBuilder.Lt(x => x.CreatedAt, cursorCreatedAt.Value),
                filterBuilder.And(
                    filterBuilder.Eq(x => x.CreatedAt, cursorCreatedAt.Value),
                    filterBuilder.Lt(x => x.Id, cursorId.Value)));
        }

        return await context.UserNotifications
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Limit(limit)
            .ToListAsync(ct);
    }
}
