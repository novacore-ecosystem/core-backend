namespace NovaCore.BuildingBlock.Contract.Events.Product;

/// <summary>Fired when a tag is assigned to a product. Consumed by Product's own Search sync (see docs/reference/search.md).</summary>
public sealed record ProductTagAssignedIntegrationEvent(
    Guid ProductId, Guid TagId, string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ProductTagAssignedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
