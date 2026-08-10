using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using NovaCore.Product.Application.Abstractions.Persistence.Products;
using NovaCore.Product.Application.Abstractions.Search;
using NovaCore.Product.Application.Features.Products.Search;

namespace NovaCore.Product.Application.Features.Products.Events.OnProductSearchSyncRequired;

/// <summary>The Search Consumer's reaction: rebuild the document from current Postgres state and upsert it. See docs/reference/search.md.</summary>
public sealed class OnProductSearchSyncRequiredHandler(
    IProductReadService productReadService,
    IProductSearchIndexer searchIndexer,
    IAppLogger<OnProductSearchSyncRequiredHandler> logger) : IInternalEventHandler<OnProductSearchSyncRequiredEvent>
{
    public async Task Handle(OnProductSearchSyncRequiredEvent @event, CancellationToken ct = default)
    {
        var product = await productReadService.GetByIdAsync(@event.ProductId, ct);
        if (product is null)
        {
            logger.Warning("Product {ProductId} no longer exists - skipping search sync", @event.ProductId);
            return;
        }

        var document = await ProductSearchProjectionBuilder.BuildAsync(product, ct);
        await searchIndexer.IndexAsync(document, ct);
    }
}
