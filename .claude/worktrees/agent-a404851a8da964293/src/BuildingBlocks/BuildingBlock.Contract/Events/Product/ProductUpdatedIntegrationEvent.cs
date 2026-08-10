namespace NovaCore.BuildingBlock.Contract.Events.Product;

/// <summary>Fired when Product-level shared info (name/slug) changes - never Variant data.</summary>
public sealed record ProductUpdatedIntegrationEvent(
    Guid ProductId,
    string Name,
    string Slug,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ProductUpdatedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
