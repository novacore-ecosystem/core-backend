namespace NovaCore.BuildingBlock.Contract.Events.Product;

/// <summary>Fired when a variation's Sku/Name/Price/Status changes - the fields catalog consumers (Order) care about.</summary>
public sealed record VariantUpdatedIntegrationEvent(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string VariationName,
    string Sku,
    decimal Price,
    string Status,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(VariantUpdatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
