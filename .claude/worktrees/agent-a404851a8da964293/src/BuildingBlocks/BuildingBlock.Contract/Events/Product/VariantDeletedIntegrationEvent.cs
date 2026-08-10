namespace NovaCore.BuildingBlock.Contract.Events.Product;

public sealed record VariantDeletedIntegrationEvent(
    Guid ProductId,
    Guid VariantId,
    string? CorrelationId = null) : IIntegrationEvent
{
    public string CorrelationId { get; } = CorrelationId ?? Guid.NewGuid().ToString();
    public string EventType { get; init; } = nameof(VariantDeletedIntegrationEvent);
    public DateTime PublishedAt { get; init; } = DateTime.UtcNow;
}
