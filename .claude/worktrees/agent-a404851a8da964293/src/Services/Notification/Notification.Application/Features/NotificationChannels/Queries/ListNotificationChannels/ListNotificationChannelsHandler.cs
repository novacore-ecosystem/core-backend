using NovaCore.Notification.Application.Abstractions.Persistence.NotificationChannels;

namespace NovaCore.Notification.Application.Features.NotificationChannels.Queries.ListNotificationChannels;

public sealed class ListNotificationChannelsHandler(INotificationChannelReadService notificationChannelReadService)
    : IQueryHandler<ListNotificationChannelsQuery, IReadOnlyList<NotificationChannelSummaryResponse>>
{
    public async Task<IReadOnlyList<NotificationChannelSummaryResponse>> Handle(ListNotificationChannelsQuery request, CancellationToken ct = default)
    {
        var items = await notificationChannelReadService.ListAsync(ct);

        return [.. items.Select(x => new NotificationChannelSummaryResponse(x.Id, x.ChannelType, x.DisplayName, x.Status, x.ValidationStatus))];
    }
}
