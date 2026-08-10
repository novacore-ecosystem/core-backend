namespace NovaCore.Notification.Application.Abstractions.Services;

/// <summary>
/// Cache-aside lookup for <see cref="NotificationChannel"/> by <see cref="NotificationChannelType"/>.
/// Channels are few (one row per type) and admin-edited rarely, so a short TTL cache in front of
/// <see cref="Repositories.INotificationChannelRepository"/> saves a DB round trip per dispatch
/// without meaningfully risking stale Enable/Disable state.
/// </summary>
public interface INotificationChannelCache
{
    Task<NotificationChannel?> GetByChannelTypeAsync(NotificationChannelType channelType, CancellationToken ct = default);

    /// <summary>Evicts one channel type - call after any command that mutates that channel's row.</summary>
    Task InvalidateAsync(NotificationChannelType channelType, CancellationToken ct = default);

    /// <summary>Evicts every cached channel entry.</summary>
    Task InvalidateAllAsync(CancellationToken ct = default);
}
