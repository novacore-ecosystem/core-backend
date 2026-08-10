using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Product;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using NovaCore.Order.Application.Features.Catalog.Events.OnVariantDeleted;

namespace NovaCore.Order.Infrastructure.Messaging.Consumers;

public sealed class VariantDeletedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<VariantDeletedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(VariantDeletedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<VariantDeletedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize VariantDeletedIntegrationEvent");

        logger.Information(
            "Received VariantDeletedIntegrationEvent for VariantId: {VariantId}",
            integrationEvent.VariantId);

        var @event = new OnVariantDeletedEvent(
            integrationEvent.ProductId,
            integrationEvent.VariantId,
            integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information(
            "Successfully processed VariantDeletedIntegrationEvent for VariantId: {VariantId}",
            integrationEvent.VariantId);
    }
}
