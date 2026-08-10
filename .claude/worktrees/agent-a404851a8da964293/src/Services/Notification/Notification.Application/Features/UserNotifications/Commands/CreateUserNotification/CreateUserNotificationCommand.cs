namespace NovaCore.Notification.Application.Features.UserNotifications.Commands.CreateUserNotification;

public sealed record CreateUserNotificationCommand(
    Guid UserId,
    string Category,
    string Type,
    string Title,
    string Body,
    NotificationPriority Priority = NotificationPriority.Normal,
    string? MetadataJson = null,
    DateTime? ExpiredAt = null,
    Guid? CampaignId = null) : ICommand<CreateUserNotificationResponse>;

public sealed record CreateUserNotificationResponse(Guid Id);
