using NovaCore.BuildingBlock.Application.Abstractions.Events;
using NovaCore.BuildingBlock.Application.Abstractions.Services;

using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;
using NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;
using NovaCore.Inventory.Application.Features.Inventories.DTOs;

namespace NovaCore.Inventory.Application.Features.Inventories.Events.OnVariantCreated;

/// <summary>
/// Every new Variant needs a stock record before it can be tracked. Defaults new
/// inventory to the well-known "PLATFORM" warehouse (seeded by InventorySeeder) at zero stock - a
/// real warehouse assignment happens later via StockIn.
/// </summary>
public sealed class OnVariantCreatedHandler(
    IInventoryReadService inventoryReadService,
    IInventoryWriteService inventoryWriteService,
    IWarehouseReadService warehouseReadService,
    IUnitOfWork unitOfWork,
    IAppLogger<OnVariantCreatedHandler> logger) : IInternalEventHandler<OnVariantCreatedEvent>
{
    private const string DefaultWarehouseCode = "PLATFORM";

    public async Task Handle(OnVariantCreatedEvent @event, CancellationToken ct = default)
    {
        var warehouse = await warehouseReadService.GetByCodeAsync(DefaultWarehouseCode, ct);
        if (warehouse is null)
        {
            logger.Warning(
                "Default warehouse {WarehouseCode} not found, skipping inventory initialization for Variant {VariantId}",
                DefaultWarehouseCode,
                @event.VariantId);
            return;
        }

        // Idempotency safety net beyond the Inbox dedup - a redelivered/replayed message must
        // never create a second zero-stock row for the same (variation, warehouse) pair.
        var existing = await inventoryReadService.GetByVariationAndWarehouseAsync(@event.VariantId, warehouse.Id, ct);
        if (existing is not null)
            return;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await inventoryWriteService.AddAsync(
                new CreateInventoryRequest(@event.ProductId, @event.VariantId, warehouse.Id),
                ct);
        }, ct: ct);
    }
}
