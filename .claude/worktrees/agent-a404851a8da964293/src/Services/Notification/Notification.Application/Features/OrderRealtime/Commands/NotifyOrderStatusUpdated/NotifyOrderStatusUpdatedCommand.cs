namespace NovaCore.Notification.Application.Features.OrderRealtime.Commands.NotifyOrderStatusUpdated;

/// <summary>
/// Dispatched by NotificationTriggerConsumer on OrderConfirmedIntegrationEvent/
/// OrderCancelledIntegrationEvent - pushes IClientSiteActions.OrderStatusUpdated to the customer
/// who owns the order. Runs alongside CreateUserNotificationCommand (Notification Center entry +
/// generic push); this one is for the frontend to patch its own order view directly, not for the
/// notification bell.
/// </summary>
public sealed record NotifyOrderStatusUpdatedCommand(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    string? Reason,
    decimal? TotalAmount) : ICommand;
