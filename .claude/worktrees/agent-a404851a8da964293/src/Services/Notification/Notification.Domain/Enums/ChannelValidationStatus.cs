namespace NovaCore.Notification.Domain.Enums;

/// <summary>Result of the last time a worker actually tested a <see cref="Entities.NotificationChannel"/>'s configuration (e.g. an SMTP connection check). Set via <see cref="Entities.NotificationChannel.RecordValidationResult"/>.</summary>
public enum ChannelValidationStatus
{
    NotValidated = 1,
    Valid = 2,
    Invalid = 3,
}
