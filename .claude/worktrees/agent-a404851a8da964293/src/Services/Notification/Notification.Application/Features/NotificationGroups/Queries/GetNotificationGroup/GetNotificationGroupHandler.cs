using NovaCore.Notification.Application.Abstractions.Persistence.NotificationGroups;

using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Notification.Application.Features.NotificationGroups.Queries.GetNotificationGroup;

public sealed class GetNotificationGroupHandler(INotificationGroupReadService notificationGroupReadService)
    : IQueryHandler<GetNotificationGroupQuery, GetNotificationGroupResponse>
{
    public async Task<GetNotificationGroupResponse> Handle(GetNotificationGroupQuery request, CancellationToken ct = default)
    {
        var entity = await notificationGroupReadService.GetByIdAsync(request.GroupId, ct)
            ?? throw new NotFoundException("NotificationGroup", request.GroupId);

        return new GetNotificationGroupResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Status,
            entity.Audience.Type,
            entity.Audience.ConfigJson,
            entity.CreatedAt);
    }
}
