using NovaCore.Notification.Application.Abstractions.Persistence.NotificationChannels;
using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.NotificationChannels.Read;

public sealed class NotificationChannelReadService(NotificationMongoContext context) : INotificationChannelReadService
{
    public async Task<NotificationChannel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.NotificationChannels.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<NotificationChannel?> GetByChannelTypeAsync(NotificationChannelType channelType, CancellationToken ct = default)
    {
        return await context.NotificationChannels.Find(x => x.ChannelType == channelType).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<NotificationChannel>> ListAsync(CancellationToken ct = default)
    {
        return await context.NotificationChannels
            .Find(Builders<NotificationChannel>.Filter.Empty)
            .SortBy(x => x.ChannelType)
            .ToListAsync(ct);
    }
}
