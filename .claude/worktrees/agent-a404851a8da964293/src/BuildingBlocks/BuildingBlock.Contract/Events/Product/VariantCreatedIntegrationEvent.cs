namespace NovaCore.BuildingBlock.Contract.Events.Product;

/// <summary>
/// Fired once per Variant - both for each initial variation at Product creation and
/// for a later standalone AddVariation call. This is the event stock-keeping actually reacts
/// to (Inventory creates one zero-stock row per variation), replacing the pre-redesign
/// ProductCreatedIntegrationEvent's single VariantId field now that a Product always has N
/// variations. ProductName is carried so Order can build its ProductCatalog row without a
/// second lookup back to Product Service. VariationName is carried alongside it (additive -
/// existing consumers that only map ProductName are unaffected) so a future consumer can start
/// displaying the variation's own name instead of the parent product's.
/// </summary>
public sealed record VariantCreatedIntegrationEvent(
    Guid ProductId,
    Guid VariantId,
    string Sku,
    string ProductName,
    string VariationName,
    decimal Price,
    string Status,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(VariantCreatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
