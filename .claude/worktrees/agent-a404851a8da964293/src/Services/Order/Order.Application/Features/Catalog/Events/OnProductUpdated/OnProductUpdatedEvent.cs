namespace NovaCore.Order.Application.Features.Catalog.Events.OnProductUpdated;

public sealed record OnProductUpdatedEvent(
    Guid ProductId,
    string Name,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
