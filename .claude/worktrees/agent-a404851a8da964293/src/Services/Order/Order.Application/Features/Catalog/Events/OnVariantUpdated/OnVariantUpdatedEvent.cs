namespace NovaCore.Order.Application.Features.Catalog.Events.OnVariantUpdated;

public sealed record OnVariantUpdatedEvent(
    Guid ProductId,
    Guid VariantId,
    string Name,
    string Sku,
    decimal Price,
    string Status,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
