namespace NovaCore.BuildingBlock.Contract.Events.Product;

public sealed record ProductDeletedIntegrationEvent(
    Guid ProductId,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(ProductDeletedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
