namespace NovaCore.Notification.Application.Features.OrderRealtime.Commands.NotifyNewOrderToAdmins;

/// <summary>Dispatched by NotificationTriggerConsumer on OrderCreatedIntegrationEvent - pushes IAdminSiteActions.OrderCreated to every connected admin.</summary>
public sealed record NotifyNewOrderToAdminsCommand(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount) : ICommand;
