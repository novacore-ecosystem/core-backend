namespace NovaCore.Order.Application.Features.Catalog.Events.OnVariantDeleted;

public sealed record OnVariantDeletedEvent(
    Guid ProductId,
    Guid VariantId,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
