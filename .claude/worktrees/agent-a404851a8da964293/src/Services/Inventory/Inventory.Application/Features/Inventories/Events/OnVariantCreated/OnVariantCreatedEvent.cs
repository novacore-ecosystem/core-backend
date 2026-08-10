using NovaCore.BuildingBlock.Application.Abstractions.Events;

namespace NovaCore.Inventory.Application.Features.Inventories.Events.OnVariantCreated;

public sealed record OnVariantCreatedEvent(
    Guid ProductId,
    Guid VariantId,
    string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
