using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;
using NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;
using NovaCore.Inventory.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Services;

/// <summary>
/// Owns the complete warehouse transfer workflow: validates both warehouses, deducts from source, receives at destination.
/// Handles multi-item transfers atomically with complete audit trail.
/// </summary>
public sealed class TransferService(
    IInventoryReadService inventoryReadService,
    IInventoryWriteService inventoryWriteService,
    IWarehouseReadService warehouseReadService,
    IInventoryDocumentService documentService,
    IInventoryTransactionService transactionService) : ITransferService
{
    /// <summary>
    /// Transfers multiple items from source warehouse to destination warehouse.
    /// Creates both Issue (source) and Receipt (destination) documents atomically.
    /// </summary>
    public async Task<ITransferService.TransferResult> TransferAsync(
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        IReadOnlyList<ITransferService.TransferItem> items,
        string reason,
        CancellationToken ct = default)
    {
        // Validate warehouses
        var sourceWarehouse = await warehouseReadService.GetByIdAsync(sourceWarehouseId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Source warehouse {sourceWarehouseId} not found.");

        var destinationWarehouse = await warehouseReadService.GetByIdAsync(destinationWarehouseId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Destination warehouse {destinationWarehouseId} not found.");

        if (sourceWarehouseId == destinationWarehouseId)
            throw ExceptionFactory.InvalidRange("Source and destination warehouse must be different.");

        var sourceInventories = new List<InventoryStock>();
        var destinationInventories = new List<InventoryStock>();
        var sourceTransactions = new List<(
            Guid InventoryId,
            Guid ProductId,
            Guid VariantId,
            Guid WarehouseId,
            InventoryTransactionType Type,
            int Quantity,
            int BalanceAfter,
            string Reason)>();
        var destinationTransactions = new List<(
            Guid InventoryId,
            Guid ProductId,
            Guid VariantId,
            Guid WarehouseId,
            InventoryTransactionType Type,
            int Quantity,
            int BalanceAfter,
            string Reason)>();

        // Process each item
        foreach (var item in items)
        {
            // Get source inventory and validate
            var sourceInventory = await inventoryReadService.GetByVariationAndWarehouseAsync(
                item.VariantId, sourceWarehouseId, ct);

            if (sourceInventory is null)
            {
                throw ExceptionFactory.EntityNotFound(
                    $"Inventory for variant {item.VariantId} in source warehouse not found.");
            }

            if (sourceInventory.AvailableQuantity < item.Quantity)
            {
                throw ExceptionFactory.InsufficientStock(
                    $"Insufficient stock for variant {item.VariantId}. " +
                    $"Available: {sourceInventory.AvailableQuantity}, Required: {item.Quantity}");
            }

            // Get or create destination inventory
            var destinationInventory = await inventoryReadService.GetByVariationAndWarehouseAsync(
                item.VariantId, destinationWarehouseId, ct);

            if (destinationInventory is null)
            {
                throw ExceptionFactory.EntityNotFound(
                    $"Inventory for variant {item.VariantId} in destination warehouse not found. " +
                    "Create inventory records in destination warehouse first.");
            }

            // Perform transfers
            var deducted = await inventoryWriteService.DeductStockAsync(
                sourceInventory.Id, item.Quantity, ct);
            sourceInventories.Add(deducted);

            var received = await inventoryWriteService.ReceiveStockAsync(
                destinationInventory.Id, item.Quantity, ct);
            destinationInventories.Add(received);

            // Record transactions
            sourceTransactions.Add((
                deducted.Id,
                deducted.ProductId,
                deducted.VariantId,
                deducted.WarehouseId,
                InventoryTransactionType.TransferOut,
                item.Quantity,
                deducted.AvailableQuantity,
                $"Transfer to {destinationWarehouse.Name}: {reason}"));

            destinationTransactions.Add((
                received.Id,
                received.ProductId,
                received.VariantId,
                received.WarehouseId,
                InventoryTransactionType.TransferIn,
                item.Quantity,
                received.AvailableQuantity,
                $"Transfer from {sourceWarehouse.Name}: {reason}"));
        }

        // Create transfer documents
        var transferId = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

        var sourceDocument = await documentService.CreateAndCompleteAsync(
            number: $"TRX-OUT-{transferId}",
            type: InventoryDocumentType.Transfer,
            reason: InventoryDocumentReason.Transfer,
            sourceWarehouseId: sourceWarehouseId,
            destinationWarehouseId: destinationWarehouseId,
            description: reason,
            ct: ct);

        var destinationDocument = await documentService.CreateAndCompleteAsync(
            number: $"TRX-IN-{transferId}",
            type: InventoryDocumentType.Transfer,
            reason: InventoryDocumentReason.Transfer,
            sourceWarehouseId: sourceWarehouseId,
            destinationWarehouseId: destinationWarehouseId,
            description: reason,
            ct: ct);

        // Add items to documents
        foreach (var item in items)
        {
            var sourceInv = sourceInventories.First(i => i.VariantId == item.VariantId);
            sourceDocument.AddItem(
                productId: sourceInv.ProductId,
                productVariantId: item.VariantId,
                quantity: item.Quantity,
                unitOfMeasure: "EA",
                description: $"Transfer to {destinationWarehouse.Name}");

            var destInv = destinationInventories.First(i => i.VariantId == item.VariantId);
            destinationDocument.AddItem(
                productId: destInv.ProductId,
                productVariantId: item.VariantId,
                quantity: item.Quantity,
                unitOfMeasure: "EA",
                description: $"Transfer from {sourceWarehouse.Name}");
        }

        // Record all transactions
        await transactionService.RecordBatchAsync(sourceTransactions, ct);
        await transactionService.RecordBatchAsync(destinationTransactions, ct);

        return new ITransferService.TransferResult(
            sourceDocument,
            destinationDocument,
            sourceInventories,
            destinationInventories);
    }
}
