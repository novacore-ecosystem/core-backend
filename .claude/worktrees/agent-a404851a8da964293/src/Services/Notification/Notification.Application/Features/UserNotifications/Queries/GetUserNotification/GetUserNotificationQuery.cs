namespace NovaCore.Notification.Application.Features.UserNotifications.Queries.GetUserNotification;

public sealed record GetUserNotificationQuery(Guid NotificationId) : IQuery<GetUserNotificationResponse>;

public sealed record GetUserNotificationResponse(
    Guid Id,
    Guid UserId,
    string Category,
    string Type,
    string Title,
    string Body,
    NotificationPriority Priority,
    NotificationStatus Status,
    DateTime? ReadAt,
    DateTime? ExpiredAt,
    Guid? CampaignId,
    DateTime CreatedAt);
