namespace NovaCore.Notification.Application.Features.NotificationChannels.Queries.ListNotificationChannels;

/// <summary>No pagination - channels are a small, effectively-fixed set (one row per deliverable NotificationChannelType), same reasoning as INotificationChannelRepository.ListAsync.</summary>
public sealed record ListNotificationChannelsQuery : IQuery<IReadOnlyList<NotificationChannelSummaryResponse>>;

public sealed record NotificationChannelSummaryResponse(
    Guid Id,
    NotificationChannelType ChannelType,
    string DisplayName,
    NotificationChannelStatus Status,
    ChannelValidationStatus ValidationStatus);
