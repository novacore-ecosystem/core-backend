using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Order;

using MediatR;

using NovaCore.Order.Application.Features.Orders.Commands.RunCreateOrderSaga;

namespace NovaCore.Order.Infrastructure.Messaging.Consumers;

/// <summary>
/// Entry point for PHASE 5/6 of the CreateOrder workflow (docs/reference/create-order-saga.md) -
/// deserializes and dispatches, no business logic here (same convention as every other consumer
/// in this codebase). Kept deliberately thin: only ISender/IAppLogger, so constructing this class
/// never touches a DbContext/gRPC client - AddKafkaMessaging's DiscoverConsumerTopics eagerly
/// constructs every registered consumer (in a synchronously-disposed scope) just to read its
/// Topics, and a consumer whose constructor pulls in a DbContext-backed IUnitOfWork (IAsyncDisposable
/// only) breaks that eager construction/disposal. The actual saga-driving logic lives in
/// RunCreateOrderSagaHandler, resolved lazily by MediatR only when this HandleAsync runs for real.
///
/// Reuses the standard Outbox -> Kafka -> Inbox path (registered like any other
/// IIntegrationEventConsumer, so IntegrationEventConsumerRegistry's generic Inbox dedup/retry
/// wrapper already makes redelivery/consumer-restart/duplicate-event handling safe) rather than
/// any saga-specific plumbing.
/// </summary>
public sealed class OrderCreatedSagaConsumer(
    ISender sender,
    IAppLogger<OrderCreatedSagaConsumer> logger) : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(OrderCreatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize OrderCreatedIntegrationEvent");

        logger.Information("Starting CreateOrderSaga for order {OrderId}", integrationEvent.OrderId);

        var command = new RunCreateOrderSagaCommand(
            integrationEvent.OrderId,
            integrationEvent.CustomerId,
            integrationEvent.Items,
            integrationEvent.CorrelationId);

        await sender.Send(command, ct);
    }
}
