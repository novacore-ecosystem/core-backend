using NovaCore.Notification.Application.Abstractions.Persistence.NotificationCampaigns;
using NovaCore.Notification.Persistence.Engine;

namespace NovaCore.Notification.Persistence.Contexts.NotificationCampaigns.Read;

public sealed class NotificationCampaignReadService(NotificationMongoContext context) : INotificationCampaignReadService
{
    public async Task<NotificationCampaign?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.NotificationCampaigns.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<NotificationCampaign> Items, int TotalCount)> SearchAsync(
        CampaignStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<NotificationCampaign>.Filter;
        var filter = filterBuilder.Empty;

        if (status is not null)
            filter &= filterBuilder.Eq(x => x.Status, status.Value);

        var totalCount = (int)await context.NotificationCampaigns.CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await context.NotificationCampaigns
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
