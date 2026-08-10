namespace NovaCore.BuildingBlock.Contract.Events.Product;

/// <summary>Fired when a category is assigned to a product. Consumed by Product's own Search sync (see docs/reference/search.md).</summary>
public sealed record ProductCategoryAssignedIntegrationEvent(
    Guid ProductId, Guid CategoryId, string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ProductCategoryAssignedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
