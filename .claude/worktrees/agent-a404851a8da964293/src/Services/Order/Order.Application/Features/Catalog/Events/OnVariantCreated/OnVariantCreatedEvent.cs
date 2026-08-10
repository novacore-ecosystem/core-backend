namespace NovaCore.Order.Application.Features.Catalog.Events.OnVariantCreated;

public sealed record OnVariantCreatedEvent(
    Guid ProductId,
    Guid VariantId,
    string Sku,
    string ProductName,
    string VariationName,
    decimal Price,
    string Status,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
