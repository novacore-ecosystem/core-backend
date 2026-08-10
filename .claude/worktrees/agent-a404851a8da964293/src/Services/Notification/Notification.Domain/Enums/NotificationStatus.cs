namespace NovaCore.Notification.Domain.Enums;

/// <summary>Lifecycle of one <see cref="Entities.UserNotification"/> (Notification Center entry).</summary>
public enum NotificationStatus
{
    Unread = 1,
    Read = 2,
    Archived = 3,
}
