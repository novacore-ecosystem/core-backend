namespace NovaCore.BuildingBlock.Contract.Events.Product;

/// <summary>
/// Product-level info only - Sku/Price live on Variant now, so stock/catalog
/// consumers react to VariantCreatedIntegrationEvent instead (see that event's
/// remarks). Consumed by Order (name for its ProductCatalog read-model).
/// </summary>
public sealed record ProductCreatedIntegrationEvent(
    Guid ProductId,
    string Code,
    string Name,
    string Slug,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ProductCreatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
