using NovaCore.Order.Application.Abstractions.Persistence.ProductCatalogs;

namespace NovaCore.Order.Application.Features.Catalog.Events.OnVariantDeleted;

public sealed class OnVariantDeletedHandler(
    IUnitOfWork uow,
    IProductCatalogWriteService catalogWriteService) : IInternalEventHandler<OnVariantDeletedEvent>
{
    public async Task Handle(OnVariantDeletedEvent @event, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(
            action: async () =>
            {
                await catalogWriteService.DeleteAsync(
                    @event.ProductId,
                    @event.VariantId,
                    ct);
            },
            ct: ct);
    }
}
