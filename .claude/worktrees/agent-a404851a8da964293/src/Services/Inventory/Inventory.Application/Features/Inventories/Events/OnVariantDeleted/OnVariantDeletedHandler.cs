using NovaCore.BuildingBlock.Application.Abstractions.Events;

using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;

namespace NovaCore.Inventory.Application.Features.Inventories.Events.OnVariantDeleted;

/// <summary>A deleted variation no longer exists to hold stock against, so its inventory rows (across every warehouse) are removed with it.</summary>
public sealed class OnVariantDeletedHandler(
    IInventoryWriteService inventoryWriteService,
    IUnitOfWork unitOfWork) : IInternalEventHandler<OnVariantDeletedEvent>
{
    public async Task Handle(OnVariantDeletedEvent @event, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryWriteService.DeleteByVariationIdAsync(@event.VariantId, ct);
        }, ct: ct);
    }
}
