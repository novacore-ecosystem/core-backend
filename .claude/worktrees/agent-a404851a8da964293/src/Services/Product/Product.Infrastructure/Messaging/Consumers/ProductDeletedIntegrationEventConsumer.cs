using System.Text.Json;

using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Product;
using NovaCore.BuildingBlock.Messaging.Abstractions;

using NovaCore.Product.Application.Features.Products.Events.OnProductSearchRemovalRequired;

namespace NovaCore.Product.Infrastructure.Messaging.Consumers;

/// <summary>The product no longer exists in Postgres - reacts with a Search index delete, not a rebuild.</summary>
public sealed class ProductDeletedIntegrationEventConsumer(
    IInternalEventDispatcher eventDispatcher,
    IAppLogger<ProductDeletedIntegrationEventConsumer> logger)
    : IIntegrationEventConsumer
{
    public IEnumerable<string> Topics => [
        nameof(ProductDeletedIntegrationEvent).ToLowerInvariant(),
    ];

    public async Task HandleAsync(
        string message, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var integrationEvent = JsonSerializer.Deserialize<ProductDeletedIntegrationEvent>(message)
            ?? throw new InvalidOperationException("Failed to deserialize ProductDeletedIntegrationEvent");

        logger.Information("Received ProductDeletedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);

        var @event = new OnProductSearchRemovalRequiredEvent(integrationEvent.ProductId, integrationEvent.CorrelationId);
        await eventDispatcher.PublishAsync(@event, ct);

        logger.Information("Successfully processed ProductDeletedIntegrationEvent for ProductId: {ProductId}", integrationEvent.ProductId);
    }
}
