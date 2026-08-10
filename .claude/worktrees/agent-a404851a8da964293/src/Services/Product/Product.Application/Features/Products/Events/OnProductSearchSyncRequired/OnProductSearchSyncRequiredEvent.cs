using NovaCore.BuildingBlock.Application.Abstractions.Events;

namespace NovaCore.Product.Application.Features.Products.Events.OnProductSearchSyncRequired;

/// <summary>
/// Raised by every Product Search consumer except the Deleted one (see
/// OnProductSearchRemovalRequired) - rebuilding the whole document from current Postgres state
/// is simpler and more correct than threading partial updates through 9 different event shapes.
/// </summary>
public sealed record OnProductSearchSyncRequiredEvent(Guid ProductId, string CorrelationId = "") : IInternalEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
