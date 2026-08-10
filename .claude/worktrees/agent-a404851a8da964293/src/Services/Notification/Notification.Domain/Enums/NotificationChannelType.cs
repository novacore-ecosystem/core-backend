namespace NovaCore.Notification.Domain.Enums;

/// <summary>
/// InApp represents the Notification Center itself (a <see cref="Entities.UserNotification"/> row
/// written directly, never queued) - every other member is a real delivery platform that goes
/// through <see cref="Entities.NotificationDispatch"/>. <see cref="Entities.NotificationChannel"/>
/// and <see cref="Entities.NotificationDispatch"/> both reject InApp in their factory methods since
/// neither runtime delivery configuration nor a dispatch queue entry makes sense for it.
/// </summary>
public enum NotificationChannelType
{
    InApp = 1,
    Email = 2,
    Telegram = 3,
    Facebook = 4,
    Zalo = 5,
    Push = 6,
    SignalR = 7,
}
