using NovaCore.Notification.Application.Abstractions.Persistence.NotificationDispatches;
using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.NotificationDispatches.Read;

public sealed class NotificationDispatchReadService(NotificationMongoContext context) : INotificationDispatchReadService
{
    public async Task<NotificationDispatch?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.NotificationDispatches.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<NotificationDispatch> Items, int TotalCount)> SearchAsync(
        DispatchStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<NotificationDispatch>.Filter;
        var filter = filterBuilder.Empty;

        if (status is not null)
            filter &= filterBuilder.Eq(x => x.Status, status.Value);

        var totalCount = (int)await context.NotificationDispatches.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await context.NotificationDispatches
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<NotificationDispatch>> GetDueForProcessingAsync(int batchSize, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var filterBuilder = Builders<NotificationDispatch>.Filter;

        var filter = filterBuilder.Or(
            filterBuilder.Eq(x => x.Status, DispatchStatus.Pending),
            filterBuilder.And(
                filterBuilder.Eq(x => x.Status, DispatchStatus.Failed),
                filterBuilder.Lte(x => x.NextRetryAt, now)));

        return await context.NotificationDispatches
            .Find(filter)
            .SortBy(x => x.CreatedAt)
            .Limit(batchSize)
            .ToListAsync(ct);
    }
}
