using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Order;
using NovaCore.BuildingBlock.Contract.Events.User;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using MediatR;

using NovaCore.Notification.Application.Features.NotificationDispatches.Commands.CreateNotificationDispatch;
using NovaCore.Notification.Application.Features.NotificationDispatches.DTOs;
using NovaCore.Notification.Application.Features.OrderRealtime.Commands.NotifyNewOrderToAdmins;
using NovaCore.Notification.Application.Features.OrderRealtime.Commands.NotifyOrderStatusUpdated;
using NovaCore.Notification.Application.Features.UserNotifications.Commands.CreateUserNotification;
using NovaCore.Notification.Domain.Enums;
using NovaCore.Notification.Domain.ValueObjects;

namespace NovaCore.Notification.Infrastructure.Messaging.Consumers;

/// <summary>
/// Single fan-in consumer for every integration event that should trigger a notification, keyed
/// off the "event-type" header instead of one consumer class per event type. Add a topic to
/// <see cref="Topics"/> and a case to <see cref="HandleAsync"/> for each new event this service
/// should react to.
///
/// Deliberately thin: only ISender/IAppLogger, no business logic and no direct SignalR/hub
/// dependency here - see docs/reference/create-order-saga.md. AddKafkaMessaging's
/// DiscoverConsumerTopics eagerly constructs every registered consumer (in a synchronously-
/// disposed temporary scope) purely to read its Topics, and everything this consumer's
/// constructor needs must already be registered by that point in NovaCore.Notification.Infrastructure's
/// DI setup - ActorHubFacade's own dependency (IHubContext, from AddSignalR()) isn't registered
/// until NovaCore.Notification.API's presentation-layer setup, which runs *after* AddInfrastructure() in
/// Program.cs. Routing the actual hub push through NotifyNewOrderToAdminsHandler/
/// NotifyOrderStatusUpdatedHandler/CreateUserNotificationHandler (resolved lazily by MediatR only
/// when a message is really processed) avoids that entirely, the same way OrderCreatedSagaConsumer
/// avoids pulling in Order's DbContext-backed IUnitOfWork.
///
/// Order events reach here via the standard Outbox -> Kafka -> Inbox path, so redelivery/consumer
/// restart/duplicate delivery are already safe (IntegrationEventConsumerRegistry's generic Inbox
/// dedup wraps every consumer, this one included).
/// </summary>
public sealed class NotificationTriggerConsumer(
    ISender sender,
    IAppLogger<NotificationTriggerConsumer> logger) : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(UserProfileCreatedIntegrationEvent).ToLowerInvariant(),
        nameof(OrderCreatedIntegrationEvent).ToLowerInvariant(),
        nameof(OrderConfirmedIntegrationEvent).ToLowerInvariant(),
        nameof(OrderCancelledIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        if (!headers.TryGetValue("event-type", out var eventType))
        {
            logger.Warning("Received a notification-trigger message with no event-type header, skipping");
            return;
        }

        switch (eventType)
        {
            case nameof(UserProfileCreatedIntegrationEvent):
                await HandleUserProfileCreatedAsync(message, ct);
                break;

            case nameof(OrderCreatedIntegrationEvent):
                await HandleOrderCreatedAsync(message, ct);
                break;

            case nameof(OrderConfirmedIntegrationEvent):
                await HandleOrderConfirmedAsync(message, ct);
                break;

            case nameof(OrderCancelledIntegrationEvent):
                await HandleOrderCancelledAsync(message, ct);
                break;

            default:
                logger.Warning("No notification mapping registered for event type {EventType}, skipping", eventType);
                return;
        }

        logger.Information("Dispatched notification(s) for {EventType}", eventType);
    }

    private async Task HandleUserProfileCreatedAsync(string message, CancellationToken ct)
    {
        var data = Deserialize<UserProfileCreatedIntegrationEvent>(message);

        var payload = new NotificationDispatchPayload(
            data.UserId,
            Category: "Account",
            Type: "Welcome",
            Title: "Welcome to NovaCore",
            Content: $"Hi {data.FirstName}, your account has been created.");

        var command = new CreateNotificationDispatchCommand(
            DispatchReference.Create(nameof(UserProfileCreatedIntegrationEvent), data.UserId.ToString()),
            [NotificationChannelType.SignalR],
            JsonSerializer.Serialize(payload));

        await sender.Send(command, ct);
    }

    /// <summary>Admin approval-queue ping (PHASE 6, Step 4) - structured, ephemeral push (IAdminSiteActions.OrderCreated), no persisted UserNotification (no single recipient for a role-wide update).</summary>
    private async Task HandleOrderCreatedAsync(string message, CancellationToken ct)
    {
        var data = Deserialize<OrderCreatedIntegrationEvent>(message);

        var command = new NotifyNewOrderToAdminsCommand(data.OrderId, data.CustomerId, data.TotalAmount);

        await sender.Send(command, ct);
    }

    private async Task HandleOrderConfirmedAsync(string message, CancellationToken ct)
    {
        var data = Deserialize<OrderConfirmedIntegrationEvent>(message);

        await sender.Send(new CreateUserNotificationCommand(
            data.CustomerId,
            Category: "Order",
            Type: "OrderConfirmed",
            Title: "Order confirmed",
            Body: $"Your order {data.OrderId} has been confirmed. Total: {data.TotalAmount:C}."), ct);

        await sender.Send(new NotifyOrderStatusUpdatedCommand(
            data.OrderId, data.CustomerId, Status: "Confirmed", Reason: null, data.TotalAmount), ct);
    }

    private async Task HandleOrderCancelledAsync(string message, CancellationToken ct)
    {
        var data = Deserialize<OrderCancelledIntegrationEvent>(message);

        await sender.Send(new CreateUserNotificationCommand(
            data.CustomerId,
            Category: "Order",
            Type: "OrderCancelled",
            Title: "Order cancelled",
            Body: $"Your order {data.OrderId} was cancelled ({data.Reason})."), ct);

        await sender.Send(new NotifyOrderStatusUpdatedCommand(
            data.OrderId, data.CustomerId, Status: "Cancelled", data.Reason, TotalAmount: null), ct);
    }

    private static T Deserialize<T>(string message) =>
        JsonSerializer.Deserialize<T>(message)
            ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name}");
}
