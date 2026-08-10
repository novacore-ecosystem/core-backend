namespace NovaCore.Notification.Application.Abstractions.Persistence.NotificationChannels;

public interface INotificationChannelReadService
{
    Task<NotificationChannel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<NotificationChannel?> GetByChannelTypeAsync(NotificationChannelType channelType, CancellationToken ct = default);

    /// <summary>Channels are few (one row per NotificationChannelType) and never paginated - returns the full set, same reasoning a lookup/reference table would.</summary>
    Task<IReadOnlyList<NotificationChannel>> ListAsync(CancellationToken ct = default);
}
