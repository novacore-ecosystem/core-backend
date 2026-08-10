namespace NovaCore.Notification.Application.Features.UserNotifications.DTOs;

/// <summary>
/// Pushed to every connected admin over SignalR (IAdminSiteActions.OrderCreated) the instant a new
/// order needs approval - ephemeral, no persisted UserNotification (no single recipient for a
/// role-wide queue update). See docs/reference/create-order-saga.md.
/// </summary>
public sealed record NewOrderNotificationDto(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    DateTime CreatedAt);
