using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;
using NovaCore.Inventory.Application.Features.Inventories.DTOs;
using NovaCore.Inventory.Persistence.Contexts.Inventories.Repositories;

namespace NovaCore.Inventory.Persistence.Contexts.Inventories.Write;

/// <summary>
/// Never calls IUnitOfWork itself. AddAsync/DeleteByProductIdAsync/DeleteByVariationIdAsync are
/// each the sole write in their caller's transaction (OnVariantCreated/OnProductDeleted/
/// OnVariantDeleted own ExecuteTransactionAsync themselves). ReceiveStockAsync/DeductStockAsync/
/// AdjustStockAsync are used only by the cross-aggregate stock-mutation handlers (AdjustStock/
/// DeductStock/RestockStock/StockIn/StockOut), which own IUnitOfWork directly and batch this with
/// InventoryTransaction writes.
/// </summary>
public sealed class InventoryWriteService(
    IInventoryRepository inventoryRepo) : IInventoryWriteService
{
    public async Task AddAsync(CreateInventoryRequest request, CancellationToken ct = default)
    {
        var entity = InventoryStock.Create(
            request.ProductId,
            request.VariationId,
            request.WarehouseId,
            Sku.Create(request.VariationId.ToString()));

        await inventoryRepo.AddAsync(entity, ct);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        await inventoryRepo.DeleteByProductIdAsync(productId, ct);
    }

    public async Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default)
    {
        await inventoryRepo.DeleteByVariationIdAsync(productVariationId, ct);
    }

    public async Task<InventoryStock> ReceiveStockAsync(Guid id, int quantity, CancellationToken ct = default)
    {
        InventoryStock? entity = null;

        await inventoryRepo.UpdateAsync(id, inv =>
        {
            inv.ReceiveStock(quantity);
            entity = inv;
        }, ct);

        return entity!;
    }

    public async Task<InventoryStock> DeductStockAsync(Guid id, int quantity, CancellationToken ct = default)
    {
        InventoryStock? entity = null;

        await inventoryRepo.UpdateAsync(id, inv =>
        {
            inv.DeductStock(quantity);
            entity = inv;
        }, ct);

        return entity!;
    }

    public async Task<InventoryAdjustmentResult> AdjustStockAsync(Guid id, int newQuantity, CancellationToken ct = default)
    {
        InventoryStock? entity = null;
        var delta = 0;

        await inventoryRepo.UpdateAsync(id, inv =>
        {
            delta = newQuantity - inv.OnHandQuantity.Value;
            inv.AdjustStock(newQuantity);
            entity = inv;
        }, ct);

        return new InventoryAdjustmentResult(entity!, delta);
    }
}
