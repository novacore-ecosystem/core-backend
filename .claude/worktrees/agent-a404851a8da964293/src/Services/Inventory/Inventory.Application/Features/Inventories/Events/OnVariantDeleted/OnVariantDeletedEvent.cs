using NovaCore.BuildingBlock.Application.Abstractions.Events;

namespace NovaCore.Inventory.Application.Features.Inventories.Events.OnVariantDeleted;

public sealed record OnVariantDeletedEvent(
    Guid ProductId,
    Guid VariantId,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
