namespace NovaCore.Notification.Application.Features.NotificationChannels.Queries.GetNotificationChannel;

public sealed record GetNotificationChannelQuery(Guid ChannelId) : IQuery<GetNotificationChannelResponse>;

public sealed record GetNotificationChannelResponse(
    Guid Id,
    NotificationChannelType ChannelType,
    string DisplayName,
    NotificationChannelStatus Status,
    string ConfigJson,
    ChannelValidationStatus ValidationStatus,
    DateTime? LastValidatedAt,
    string? LastValidationError);
