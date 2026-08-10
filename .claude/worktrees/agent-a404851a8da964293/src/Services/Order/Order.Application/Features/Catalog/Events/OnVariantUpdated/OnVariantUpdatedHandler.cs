using NovaCore.BuildingBlock.Application.Abstractions.Services;

using NovaCore.Order.Application.Abstractions.Persistence.ProductCatalogs;

namespace NovaCore.Order.Application.Features.Catalog.Events.OnVariantUpdated;

public sealed class OnVariantUpdatedHandler(
    IUnitOfWork uow,
    IProductCatalogReadService catalogReadService,
    IProductCatalogWriteService catalogWriteService,
    IAppLogger<OnVariantUpdatedHandler> logger) : IInternalEventHandler<OnVariantUpdatedEvent>
{
    public async Task Handle(OnVariantUpdatedEvent @event, CancellationToken ct = default)
    {
        var exists = await catalogReadService.ExistsAsync(@event.VariantId, ct);
        if (!exists)
        {
            logger.Warning(
                "ProductCatalog entry not found for VariantId {VariantId}, skipping update",
                @event.VariantId);
            return;
        }

        await uow.ExecuteTransactionAsync(
            action: async () =>
            {
                await catalogWriteService.UpdateVariationSnapshotAsync(
                    @event.ProductId,
                    @event.VariantId,
                    @event.Name,
                    Sku.Create(@event.Sku),
                    Money.Create(@event.Price),
                    Enum.Parse<ProductCatalogStatus>(@event.Status),
                    ct);
            },
            ct: ct);
    }
}
