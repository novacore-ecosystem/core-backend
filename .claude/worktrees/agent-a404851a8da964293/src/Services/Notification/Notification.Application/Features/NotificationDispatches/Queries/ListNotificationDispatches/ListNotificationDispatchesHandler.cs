using NovaCore.BuildingBlock.Application.Abstractions.Common;

using NovaCore.Notification.Application.Abstractions.Persistence.NotificationDispatches;

namespace NovaCore.Notification.Application.Features.NotificationDispatches.Queries.ListNotificationDispatches;

public sealed class ListNotificationDispatchesHandler(INotificationDispatchReadService notificationDispatchReadService)
    : IQueryHandler<ListNotificationDispatchesQuery, PaginatedResult<NotificationDispatchSummaryResponse>>
{
    public async Task<PaginatedResult<NotificationDispatchSummaryResponse>> Handle(ListNotificationDispatchesQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await notificationDispatchReadService.SearchAsync(request.Status, request.Page, request.PageSize, ct);

        var mapped = items
            .Select(x => new NotificationDispatchSummaryResponse(
                x.Id, x.Reference.ReferenceType, x.Reference.ReferenceId, x.Channel, x.Status, x.RetryCount, x.CreatedAt))
            .ToList();

        return PaginatedResult<NotificationDispatchSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
