using NovaCore.Notification.Application.Features.UserNotifications.DTOs;

namespace NovaCore.Notification.Application.Abstractions.Services;

/// <summary>
/// Port onto the realtime push side (SignalR today) - kept separate from IChannelSender/
/// NotificationDispatch's queue-and-retry delivery model because these pushes are always
/// best-effort/instant (see CreateUserNotificationHandler, NotifyOrderStatusUpdatedHandler,
/// NotifyNewOrderToAdminsHandler): a client that's disconnected simply misses the push, there's
/// nothing to retry.
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>Generic Notification Center push (IGlobalHubBase.ReceiveNotification) - fires alongside every persisted UserNotification.</summary>
    Task PushToUserAsync(Guid userId, NotificationDto notification, CancellationToken ct = default);

    /// <summary>Structured order-status push to the one customer who owns the order (IClientSiteActions.OrderStatusUpdated).</summary>
    Task PushOrderStatusUpdatedAsync(Guid userId, OrderStatusUpdatedDto notification, CancellationToken ct = default);

    /// <summary>Structured new-order push to every connected admin (IAdminSiteActions.OrderCreated).</summary>
    Task PushNewOrderToAdminsAsync(NewOrderNotificationDto notification, CancellationToken ct = default);
}
