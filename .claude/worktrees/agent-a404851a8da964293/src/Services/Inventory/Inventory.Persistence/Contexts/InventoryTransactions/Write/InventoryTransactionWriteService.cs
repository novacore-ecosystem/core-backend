using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using NovaCore.Inventory.Application.Features.Inventories.DTOs;
using NovaCore.Inventory.Persistence.Contexts.InventoryTransactions.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.InventoryTransactions.Write;

public sealed class InventoryTransactionWriteService(
    IInventoryTransactionRepository repo) : IInventoryTransactionWriteService
{
    public async Task StageAddAsync(CreateInventoryTransactionDto request, CancellationToken ct = default)
    {
        var entity = InventoryTransaction.Create(
            inventoryId: request.InventoryId,
            warehouseId: request.WarehouseId,
            productId: request.ProductId,
            variantId: request.VariantId,
            type: request.Type,
            quantity: request.Quantity,
            beforeOnHandQuantity: request.QuantityAfter - request.Quantity,
            afterOnHandQuantity: request.QuantityAfter,
            beforeReservedQuantity: 0,
            afterReservedQuantity: 0,
            description: request.Reason);

        await repo.AddAsync(entity, ct);
    }
}
