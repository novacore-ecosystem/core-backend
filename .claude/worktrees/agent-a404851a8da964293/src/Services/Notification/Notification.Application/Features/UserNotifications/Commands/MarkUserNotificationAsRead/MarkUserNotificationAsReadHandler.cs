using NovaCore.Notification.Application.Abstractions.Persistence.UserNotifications;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

namespace NovaCore.Notification.Application.Features.UserNotifications.Commands.MarkUserNotificationAsRead;

public sealed class MarkUserNotificationAsReadHandler(
    ICurrentUserService currentUser,
    IUserNotificationReadService userNotificationReadService,
    IUserNotificationWriteService userNotificationWriteService) : ICommandHandler<MarkUserNotificationAsReadCommand>
{
    public async Task Handle(MarkUserNotificationAsReadCommand request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId()
            ?? throw new UnauthorizedException();

        var entity = await userNotificationReadService.GetByIdAsync(request.NotificationId, ct)
            ?? throw new NotFoundException("UserNotification", request.NotificationId);

        if (entity.UserId != userId)
            throw new ForbiddenException();

        entity.MarkAsRead();

        await userNotificationWriteService.UpdateAsync(entity, ct);
    }
}
