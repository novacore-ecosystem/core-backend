using NovaCore.Order.Application.Abstractions.Persistence.ProductCatalogs;

namespace NovaCore.Order.Application.Features.Catalog.Events.OnProductDeleted;

public sealed class OnProductDeletedHandler(
    IUnitOfWork uow,
    IProductCatalogWriteService catalogWriteService) : IInternalEventHandler<OnProductDeletedEvent>
{
    public async Task Handle(OnProductDeletedEvent @event, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(
            action: async () =>
            {
                await catalogWriteService.DeleteByProductIdAsync(@event.ProductId, ct);
            },
            ct: ct);
    }
}
