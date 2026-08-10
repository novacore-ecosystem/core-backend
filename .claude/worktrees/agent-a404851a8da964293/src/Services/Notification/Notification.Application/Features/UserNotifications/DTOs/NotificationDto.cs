namespace NovaCore.Notification.Application.Features.UserNotifications.DTOs;

public sealed record NotificationDto(
    Guid UserId,
    string Category,
    string Type,
    string Title,
    string Content,
    string Metadata,
    NotificationPriority Priority,
    NotificationStatus Status,
    DateTime ExpiredAt
);
