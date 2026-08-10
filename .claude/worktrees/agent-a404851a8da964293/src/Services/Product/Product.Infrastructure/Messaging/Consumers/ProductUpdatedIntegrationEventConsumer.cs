using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Product;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using NovaCore.Product.Application.Features.Products.Events.OnProductSearchSyncRequired;

namespace NovaCore.Product.Infrastructure.Messaging.Consumers;

public sealed class ProductUpdatedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<ProductUpdatedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(ProductUpdatedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<ProductUpdatedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize ProductUpdatedIntegrationEvent");

        logger.Information("Received ProductUpdatedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);

        var @event = new OnProductSearchSyncRequiredEvent(integrationEvent.ProductId, integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Successfully processed ProductUpdatedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);
    }
}
