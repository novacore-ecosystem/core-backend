namespace NovaCore.BuildingBlock.Contract.Events.Product;

/// <summary>Fired when a tag is removed from a product. Consumed by Product's own Search sync (see docs/reference/search.md).</summary>
public sealed record ProductTagRemovedIntegrationEvent(
    Guid ProductId, Guid TagId, string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ProductTagRemovedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
