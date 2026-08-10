using NovaCore.BuildingBlock.Application.Abstractions.Common;

using NovaCore.Notification.Application.Abstractions.Persistence.NotificationGroups;

namespace NovaCore.Notification.Application.Features.NotificationGroups.Queries.ListNotificationGroups;

public sealed class ListNotificationGroupsHandler(INotificationGroupReadService notificationGroupReadService)
    : IQueryHandler<ListNotificationGroupsQuery, PaginatedResult<NotificationGroupSummaryResponse>>
{
    public async Task<PaginatedResult<NotificationGroupSummaryResponse>> Handle(ListNotificationGroupsQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await notificationGroupReadService.SearchAsync(request.Search, request.Page, request.PageSize, ct);

        var mapped = items
            .Select(x => new NotificationGroupSummaryResponse(x.Id, x.Name, x.Status, x.Audience.Type, x.CreatedAt))
            .ToList();

        return PaginatedResult<NotificationGroupSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
