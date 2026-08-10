using NovaCore.Order.Application.Abstractions.Persistence.ProductCatalogs;

namespace NovaCore.Order.Application.Features.Catalog.Events.OnProductUpdated;

/// <summary>A Product's Name is shared across every one of its variations, so this refreshes every ProductCatalog row for the product, not just one.</summary>
public sealed class OnProductUpdatedHandler(
    IUnitOfWork uow,
    IProductCatalogWriteService catalogWriteService) : IInternalEventHandler<OnProductUpdatedEvent>
{
    public async Task Handle(OnProductUpdatedEvent @event, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(
            action: async () =>
            {
                await catalogWriteService.UpdateProductNameByProductIdAsync(
                    @event.ProductId,
                    @event.Name,
                    ct);
            },
            ct: ct);
    }
}
