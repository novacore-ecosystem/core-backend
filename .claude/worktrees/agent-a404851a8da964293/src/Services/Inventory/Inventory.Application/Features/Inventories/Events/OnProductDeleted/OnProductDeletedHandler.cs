using NovaCore.BuildingBlock.Application.Abstractions.Events;

using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;

namespace NovaCore.Inventory.Application.Features.Inventories.Events.OnProductDeleted;

/// <summary>
/// Whole-product deletion is an EF cascade over the owned Variant rows (see
/// Product.RemoveVariation vs. a full aggregate delete), so no per-variation Deleted event fires
/// for each one - this handler is the fallback that cleans up every inventory row for the
/// product in one pass.
/// </summary>
public sealed class OnProductDeletedHandler(
    IInventoryWriteService inventoryWriteService,
    IUnitOfWork unitOfWork) : IInternalEventHandler<OnProductDeletedEvent>
{
    public async Task Handle(OnProductDeletedEvent @event, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryWriteService.DeleteByProductIdAsync(@event.ProductId, ct);
        }, ct: ct);
    }
}
