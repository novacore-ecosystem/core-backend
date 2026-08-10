using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using NovaCore.Inventory.Application.Abstractions.Services;
using NovaCore.Inventory.Application.Features.Inventories.DTOs;

namespace NovaCore.Inventory.Application.Services;

/// <summary>
/// Records inventory transactions alongside stock changes.
/// Ensures transaction history is never forgotten and remains consistent with stock modifications.
/// </summary>
public sealed class InventoryTransactionService(
    IInventoryTransactionWriteService writeService) : IInventoryTransactionService
{
    /// <summary>
    /// Records an inventory transaction for a stock change.
    /// Call this immediately after modifying InventoryStock in the same transaction.
    /// </summary>
    public async Task RecordAsync(
        Guid inventoryId,
        Guid productId,
        Guid productVariantId,
        Guid warehouseId,
        InventoryTransactionType type,
        int quantity,
        int balanceAfter,
        string reason,
        CancellationToken ct = default)
    {
        var dto = new CreateInventoryTransactionDto(
            InventoryId: inventoryId,
            ProductId: productId,
            VariantId: productVariantId,
            WarehouseId: warehouseId,
            Type: type,
            Quantity: quantity,
            QuantityAfter: balanceAfter,
            Reason: reason);

        await writeService.StageAddAsync(dto, ct);
    }

    /// <summary>
    /// Records multiple transactions in batch (e.g., multi-item order deduction).
    /// Reduces boilerplate when recording multiple related changes.
    /// </summary>
    public async Task RecordBatchAsync(
        IReadOnlyList<(
            Guid InventoryId,
            Guid ProductId,
            Guid VariantId,
            Guid WarehouseId,
            InventoryTransactionType Type,
            int Quantity,
            int BalanceAfter,
            string Reason)> transactions,
        CancellationToken ct = default)
    {
        foreach (var (inventoryId, productId, productVariantId, warehouseId, type, quantity, balanceAfter, reason) in transactions)
        {
            await RecordAsync(inventoryId, productId, productVariantId, warehouseId, type, quantity, balanceAfter, reason, ct);
        }
    }
}
