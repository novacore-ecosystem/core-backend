using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Product;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using NovaCore.Inventory.Application.Features.Inventories.Events.OnVariantCreated;

namespace NovaCore.Inventory.Infrastructure.Messaging.Consumers;

public sealed class VariantCreatedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<VariantCreatedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(VariantCreatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        // Deliberately not caught here: the Inbox attempt executor needs the exception to
        // propagate so it can record the failure and schedule a retry.
        var integrationEvent = JsonSerializer.Deserialize<VariantCreatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize VariantCreatedIntegrationEvent");

        logger.Information(
            "Received VariantCreatedIntegrationEvent for VariantId: {VariantId}",
            integrationEvent.VariantId);

        var @event = new OnVariantCreatedEvent(
            integrationEvent.ProductId,
            integrationEvent.VariantId,
            integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed VariantCreatedIntegrationEvent for VariantId: {VariantId}",
            integrationEvent.VariantId);
    }
}
