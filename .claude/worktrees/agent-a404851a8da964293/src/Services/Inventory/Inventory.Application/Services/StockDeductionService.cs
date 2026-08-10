using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;
using NovaCore.Inventory.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Services;

/// <summary>
/// Owns the complete stock deduction workflow.
/// Coordinates inventory adjustments, document creation, and transaction recording.
/// Eliminates duplication across multiple handlers (DeductStock, StockOut, etc.).
/// </summary>
public sealed class StockDeductionService(
    IInventoryWriteService inventoryWriteService,
    IInventoryDocumentService documentService,
    IInventoryTransactionService transactionService) : IStockDeductionService
{

    /// <summary>
    /// Deducts stock for multiple items and records the complete operation.
    /// Returns document and deducted inventories for caller to use in their workflow.
    /// </summary>
    public async Task<IStockDeductionService.StockDeductionResult> DeductAsync(
        string documentNumber,
        InventoryDocumentType documentType,
        InventoryDocumentReason documentReason,
        Guid sourceWarehouseId,
        IReadOnlyList<(Guid InventoryId, Guid ProductId, Guid VariantId, int Quantity)> items,
        string description,
        CancellationToken ct = default)
    {
        var deductedInventories = new List<InventoryStock>(items.Count);
        var transactions = new List<(
            Guid InventoryId,
            Guid ProductId,
            Guid VariantId,
            Guid WarehouseId,
            InventoryTransactionType Type,
            int Quantity,
            int BalanceAfter,
            string Reason)>();

        foreach (var (inventoryId, productId, productVariantId, quantity) in items)
        {
            var deductedInventory = await inventoryWriteService.DeductStockAsync(inventoryId, quantity, ct);
            deductedInventories.Add(deductedInventory);

            transactions.Add((
                deductedInventory.Id,
                deductedInventory.ProductId,
                deductedInventory.VariantId,
                deductedInventory.WarehouseId,
                InventoryTransactionType.Deduction,
                quantity,
                deductedInventory.AvailableQuantity,
                description));
        }

        var document = await documentService.CreateAndCompleteAsync(
            number: documentNumber,
            type: documentType,
            reason: documentReason,
            sourceWarehouseId: sourceWarehouseId,
            destinationWarehouseId: null,
            description: description,
            ct: ct);

        foreach (var item in items)
        {
            document.AddItem(
                productId: item.ProductId,
                productVariantId: item.VariantId,
                quantity: item.Quantity,
                unitOfMeasure: "EA",
                inventoryId: item.InventoryId,
                description: description);
        }

        await transactionService.RecordBatchAsync(transactions, ct);

        return new IStockDeductionService.StockDeductionResult(
            Success: true,
            Document: document,
            DeductedInventories: deductedInventories);
    }
}
