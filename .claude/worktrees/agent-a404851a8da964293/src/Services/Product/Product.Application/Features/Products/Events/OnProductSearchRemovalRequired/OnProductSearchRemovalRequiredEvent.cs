using NovaCore.BuildingBlock.Application.Abstractions.Events;

namespace NovaCore.Product.Application.Features.Products.Events.OnProductSearchRemovalRequired;

/// <summary>Raised only by the ProductDeleted Search consumer - the product no longer exists in Postgres, so the reaction is a delete, not a rebuild.</summary>
public sealed record OnProductSearchRemovalRequiredEvent(Guid ProductId, string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
