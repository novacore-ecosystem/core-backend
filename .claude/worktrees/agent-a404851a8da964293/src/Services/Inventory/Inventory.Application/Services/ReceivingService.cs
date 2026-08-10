using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryLots;
using NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;
using NovaCore.Inventory.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Services;

/// <summary>
/// Owns the complete receiving workflow: validates warehouse, receives stock, creates lots, records transactions.
/// Handles multi-item receiving with lot allocation.
/// </summary>
public sealed class ReceivingService(
    IInventoryReadService inventoryReadService,
    IInventoryWriteService inventoryWriteService,
    IInventoryLotWriteService lotWriteService,
    IWarehouseReadService warehouseReadService,
    IInventoryDocumentService documentService,
    IInventoryTransactionService transactionService) : IReceivingService
{
    /// <summary>
    /// Receives multiple items into a warehouse, optionally creating lot records for lot-tracked items.
    /// </summary>
    public async Task<IReceivingService.ReceivingResult> ReceiveAsync(
        string purchaseOrderNumber,
        Guid warehouseId,
        IReadOnlyList<IReceivingService.ReceivingItem> items,
        string description,
        CancellationToken ct = default)
    {
        var warehouse = await warehouseReadService.GetByIdAsync(warehouseId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Warehouse {warehouseId} not found.");

        var receivedInventories = new List<InventoryStock>();
        var createdLots = new List<InventoryLot>();
        var transactions = new List<(
            Guid InventoryId,
            Guid ProductId,
            Guid VariantId,
            Guid WarehouseId,
            InventoryTransactionType Type,
            int Quantity,
            int BalanceAfter,
            string Reason)>();

        // Process each received item
        foreach (var item in items)
        {
            var inventory = await inventoryReadService.GetByVariationAndWarehouseAsync(
                item.VariantId, warehouseId, ct);

            if (inventory is null)
            {
                throw ExceptionFactory.EntityNotFound(
                    $"Inventory for variation {item.VariantId} in warehouse {warehouseId} not found. " +
                    "Create inventory records first.");
            }

            var receivedInventory = await inventoryWriteService.ReceiveStockAsync(
                inventory.Id, item.Quantity, ct);
            receivedInventories.Add(receivedInventory);

            // Create lot record if lot number provided (for lot-tracked items)
            if (!string.IsNullOrWhiteSpace(item.LotNumber))
            {
                var lotRequest = new CreateInventoryLotRequest(
                    InventoryId: inventory.Id,
                    LotNumber: item.LotNumber,
                    ManufactureDate: item.ManufactureDate ?? DateTime.UtcNow,
                    ExpiredDate: item.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
                    Quantity: item.Quantity);

                await lotWriteService.AddAsync(lotRequest, ct);
            }

            transactions.Add((
                receivedInventory.Id,
                receivedInventory.ProductId,
                receivedInventory.VariantId,
                receivedInventory.WarehouseId,
                InventoryTransactionType.Receipt,
                item.Quantity,
                receivedInventory.AvailableQuantity,
                $"Received from PO {purchaseOrderNumber}: {description}"));
        }

        // Create receiving document
        var document = await documentService.CreateAndCompleteAsync(
            type: InventoryDocumentType.Receipt,
            reason: InventoryDocumentReason.Purchase,
            sourceWarehouseId: null,
            destinationWarehouseId: warehouseId,
            description: $"PO {purchaseOrderNumber}: {description}",
            ct: ct);

        // Add items to document
        foreach (var item in items)
        {
            document.AddItem(
                productId: receivedInventories
                    .First(i => i.VariantId == item.VariantId).ProductId,
                productVariantId: item.VariantId,
                quantity: item.Quantity,
                unitOfMeasure: "EA",
                description: item.LotNumber ?? description);
        }

        // Record all transactions
        await transactionService.RecordBatchAsync(transactions, ct);

        return new IReceivingService.ReceivingResult(document, receivedInventories, createdLots);
    }
}
