using NovaCore.BuildingBlock.Application.Abstractions.Events;

using NovaCore.Product.Application.Abstractions.Search;

namespace NovaCore.Product.Application.Features.Products.Events.OnProductSearchRemovalRequired;

public sealed class OnProductSearchRemovalRequiredHandler(IProductSearchIndexer searchIndexer)
    : IInternalEventHandler<OnProductSearchRemovalRequiredEvent>
{
    public Task Handle(OnProductSearchRemovalRequiredEvent @event, CancellationToken ct = default)
        => searchIndexer.DeleteAsync(@event.ProductId, ct);
}
