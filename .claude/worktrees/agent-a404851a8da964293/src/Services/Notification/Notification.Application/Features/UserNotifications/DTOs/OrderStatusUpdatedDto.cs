namespace NovaCore.Notification.Application.Features.UserNotifications.DTOs;

/// <summary>
/// Pushed to the customer over SignalR (IClientSiteActions.OrderStatusUpdated) whenever an order
/// reaches Confirmed or Cancelled - lets the frontend patch its own order state directly instead
/// of parsing NotificationDto's free-text Content. Reason/TotalAmount are optional since
/// OrderCancelledIntegrationEvent doesn't carry a total. See docs/reference/create-order-saga.md.
/// </summary>
public sealed record OrderStatusUpdatedDto(
    Guid OrderId,
    Guid CustomerId,
    string Status,
    string? Reason,
    decimal? TotalAmount,
    DateTime UpdatedAt);
