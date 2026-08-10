using NovaCore.Notification.Application.Abstractions.Persistence.UserNotifications;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Notification.Application.Features.UserNotifications.Queries.GetUserNotification;

public sealed class GetUserNotificationHandler(
    ICurrentUserService currentUser,
    IUserNotificationReadService userNotificationReadService) : IQueryHandler<GetUserNotificationQuery, GetUserNotificationResponse>
{
    public async Task<GetUserNotificationResponse> Handle(GetUserNotificationQuery request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId()
            ?? throw new UnauthorizedException();

        var entity = await userNotificationReadService.GetByIdAsync(request.NotificationId, ct)
            ?? throw new NotFoundException("UserNotification", request.NotificationId);

        if (entity.UserId != userId)
            throw new ForbiddenException();

        return new GetUserNotificationResponse(
            entity.Id,
            entity.UserId,
            entity.Category.Value,
            entity.Type.Value,
            entity.Content.Title,
            entity.Content.Body,
            entity.Priority,
            entity.Status,
            entity.ReadAt,
            entity.ExpiredAt,
            entity.CampaignId,
            entity.CreatedAt);
    }
}
