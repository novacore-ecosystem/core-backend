using NovaCore.Notification.Application.Abstractions.Persistence.NotificationGroups;
using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.NotificationGroups.Read;

public sealed class NotificationGroupReadService(NotificationMongoContext context) : INotificationGroupReadService
{
    public async Task<NotificationGroup?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.NotificationGroups.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<NotificationGroup> Items, int TotalCount)> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<NotificationGroup>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrWhiteSpace(search))
            filter &= filterBuilder.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(search, "i"));

        var totalCount = (int)await context.NotificationGroups.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await context.NotificationGroups
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
