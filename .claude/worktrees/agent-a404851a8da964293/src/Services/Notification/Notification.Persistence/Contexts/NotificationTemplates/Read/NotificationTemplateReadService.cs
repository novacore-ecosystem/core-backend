using NovaCore.Notification.Application.Abstractions.Persistence.NotificationTemplates;
using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.NotificationTemplates.Read;

public sealed class NotificationTemplateReadService(NotificationMongoContext context) : INotificationTemplateReadService
{
    public async Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.NotificationTemplates.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<NotificationTemplate> Items, int TotalCount)> SearchAsync(
        NotificationChannelType? channel,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<NotificationTemplate>.Filter;
        var filter = filterBuilder.Empty;

        if (channel is not null)
            filter &= filterBuilder.Eq(x => x.Channel, channel.Value);

        var totalCount = (int)await context.NotificationTemplates.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await context.NotificationTemplates
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
