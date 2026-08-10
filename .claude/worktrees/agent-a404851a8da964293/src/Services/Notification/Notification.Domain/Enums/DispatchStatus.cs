namespace NovaCore.Notification.Domain.Enums;

/// <summary>Lifecycle of one <see cref="Entities.NotificationDispatch"/> (Outbox-like delivery request).</summary>
public enum DispatchStatus
{
    Pending = 1,
    Processing = 2,
    Sent = 3,
    Failed = 4,
    DeadLettered = 5,
}
