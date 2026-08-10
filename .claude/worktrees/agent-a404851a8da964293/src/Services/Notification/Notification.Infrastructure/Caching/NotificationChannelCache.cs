using NovaCore.BuildingBlock.SharedKernel.Constants;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

using NovaCore.Notification.Application.Abstractions.Persistence.NotificationChannels;
using NovaCore.Notification.Application.Abstractions.Services;
using NovaCore.Notification.Domain.Entities;
using NovaCore.Notification.Domain.Enums;

namespace NovaCore.Notification.Infrastructure.Caching;

/// <summary>
/// In-process (not Redis) cache-aside for NotificationChannel lookups. Deliberately not routed
/// through the shared ICacheService/Redis - the dataset is a handful of admin-edited rows (one
/// per NotificationChannelType), so per-instance staleness within a short TTL is a non-issue, and
/// this service has no other reason to depend on Redis. Populated on miss from
/// INotificationChannelReadService; invalidated explicitly by the three handlers that mutate a
/// channel (Enable/Disable/UpdateConfiguration) rather than relying on TTL alone.
/// </summary>
public sealed class NotificationChannelCache(
    IMemoryCache cache,
    INotificationChannelReadService channelReadService,
    IConfiguration configuration) : INotificationChannelCache
{
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(
        configuration.GetSection("Caching:EntityTtl:NotificationChannels:MinutesToExpire").Get<int?>()
            ?? CacheKeyConstant.NotificationChannels.DefaultTtlMinutes);

    public async Task<NotificationChannel?> GetByChannelTypeAsync(NotificationChannelType channelType, CancellationToken ct = default)
    {
        var key = CacheKeyConstant.NotificationChannels.ByType(channelType.ToString());
        if (cache.TryGetValue(key, out NotificationChannel? cached))
            return cached;

        var channel = await channelReadService.GetByChannelTypeAsync(channelType, ct);
        cache.Set(key, channel, _ttl);
        return channel;
    }

    public Task InvalidateAsync(NotificationChannelType channelType, CancellationToken ct = default)
    {
        cache.Remove(CacheKeyConstant.NotificationChannels.ByType(channelType.ToString()));
        return Task.CompletedTask;
    }

    public Task InvalidateAllAsync(CancellationToken ct = default)
    {
        foreach (var channelType in Enum.GetValues<NotificationChannelType>())
            cache.Remove(CacheKeyConstant.NotificationChannels.ByType(channelType.ToString()));

        return Task.CompletedTask;
    }
}
