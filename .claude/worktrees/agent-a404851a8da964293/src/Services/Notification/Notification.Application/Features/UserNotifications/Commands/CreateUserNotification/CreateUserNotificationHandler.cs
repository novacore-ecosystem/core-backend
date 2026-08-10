using NovaCore.Notification.Application.Abstractions.Persistence.UserNotifications;
using NovaCore.Notification.Application.Abstractions.Services;
using NovaCore.Notification.Application.Features.UserNotifications.DTOs;

namespace NovaCore.Notification.Application.Features.UserNotifications.Commands.CreateUserNotification;

public sealed class CreateUserNotificationHandler(
    IUserNotificationWriteService userNotificationWriteService,
    IRealtimeNotifier realtimeNotifier) : ICommandHandler<CreateUserNotificationCommand, CreateUserNotificationResponse>
{
    public async Task<CreateUserNotificationResponse> Handle(CreateUserNotificationCommand request, CancellationToken ct = default)
    {
        var content = NotificationContent.Create(request.Title, request.Body);
        var category = NotificationCategory.Create(request.Category);
        var type = NotificationType.Create(request.Type);

        var entity = UserNotification.Create(
            Guid.CreateVersion7(),
            request.UserId,
            category,
            type,
            content,
            request.Priority,
            request.MetadataJson,
            request.ExpiredAt,
            request.CampaignId);

        await userNotificationWriteService.CreateAsync(entity, ct);

        // Best-effort realtime push on top of the durable Notification Center row just persisted -
        // every creation path (this command is shared by direct API calls and event-triggered
        // notifications alike) benefits from an instant update for a connected client; a dropped
        // push for a disconnected one is harmless, the row above is what they see on next load.
        var dto = new NotificationDto(
            entity.UserId,
            category.Value,
            type.Value,
            content.Title,
            content.Body,
            Metadata: request.MetadataJson ?? "{}",
            entity.Priority,
            entity.Status,
            ExpiredAt: entity.ExpiredAt ?? DateTime.UtcNow.AddDays(30));

        await realtimeNotifier.PushToUserAsync(entity.UserId, dto, ct);

        return new CreateUserNotificationResponse(entity.Id);
    }
}
