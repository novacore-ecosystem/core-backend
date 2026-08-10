namespace NovaCore.Order.Application.Features.Catalog.Events.OnProductDeleted;

public sealed record OnProductDeletedEvent(Guid ProductId, string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
