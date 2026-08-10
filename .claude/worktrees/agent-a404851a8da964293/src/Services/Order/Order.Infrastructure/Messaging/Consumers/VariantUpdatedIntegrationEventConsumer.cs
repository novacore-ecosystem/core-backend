using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Product;

using NovaCore.Order.Application.Features.Catalog.Events.OnVariantUpdated;

namespace NovaCore.Order.Infrastructure.Messaging.Consumers;

public sealed class VariantUpdatedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<VariantUpdatedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(VariantUpdatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<VariantUpdatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize VariantUpdatedIntegrationEvent");

        logger.Information(
            "Received VariantUpdatedIntegrationEvent for VariantId: {VariantId}",
            integrationEvent.VariantId);

        var @event = new OnVariantUpdatedEvent(
            integrationEvent.ProductId,
            integrationEvent.VariantId,
            integrationEvent.VariationName,
            integrationEvent.Sku,
            integrationEvent.Price,
            integrationEvent.Status,
            integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed VariantUpdatedIntegrationEvent for VariantId: {VariantId}",
            integrationEvent.VariantId);
    }
}
