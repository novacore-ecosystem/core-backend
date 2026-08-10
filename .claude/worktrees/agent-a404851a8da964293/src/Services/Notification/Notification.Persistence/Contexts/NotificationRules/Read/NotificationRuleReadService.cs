using NovaCore.Notification.Application.Abstractions.Persistence.NotificationRules;
using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.NotificationRules.Read;

public sealed class NotificationRuleReadService(NotificationMongoContext context) : INotificationRuleReadService
{
    public async Task<NotificationRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.NotificationRules.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<NotificationRule> Items, int TotalCount)> SearchAsync(
        string? eventType,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<NotificationRule>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrWhiteSpace(eventType))
            filter &= filterBuilder.Eq(x => x.EventType, eventType);

        var totalCount = (int)await context.NotificationRules.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await context.NotificationRules
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
