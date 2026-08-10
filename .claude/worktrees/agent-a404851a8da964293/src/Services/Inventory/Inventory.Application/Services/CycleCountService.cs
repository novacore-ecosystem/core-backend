using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.Exceptions;

using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryCounts;
using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;
using NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;
using NovaCore.Inventory.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Services;

/// <summary>
/// Owns the complete cycle count workflow: create count document, record items, calculate variances, auto-adjust stock.
/// Handles inventory reconciliation with comprehensive variance tracking and adjustment.
/// </summary>
public sealed class CycleCountService(
    IInventoryReadService inventoryReadService,
    IInventoryWriteService inventoryWriteService,
    IInventoryCountReadService countReadService,
    IInventoryCountWriteService countWriteService,
    IWarehouseReadService warehouseReadService,
    IInventoryDocumentService documentService,
    IInventoryTransactionService transactionService,
    ICurrentUserService currentUser) : ICycleCountService
{
    /// <summary>
    /// Creates a new cycle count document for a warehouse.
    /// </summary>
    public async Task<InventoryCount> StartCountAsync(
        Guid warehouseId,
        DateTime countDate,
        string description,
        CancellationToken ct = default)
    {
        _ = await warehouseReadService.GetByIdAsync(warehouseId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Warehouse {warehouseId} not found.");

        var count = InventoryCount.Create(
            number: GenerateCountNumber(),
            warehouseId: warehouseId,
            countDate: countDate,
            description: description);

        await countWriteService.AddAsync(count, ct);

        return count;
    }

    /// <summary>
    /// Completes a cycle count, calculates variances, and auto-adjusts stock for discrepancies.
    /// </summary>
    public async Task<ICycleCountService.CycleCountResult> CompleteCountAsync(
        Guid countId,
        IReadOnlyList<ICycleCountService.CountItem> countedItems,
        decimal varianceThresholdPercent = 5m,
        CancellationToken ct = default)
    {
        var approvedBy = currentUser.GetUserId() ?? throw new ForbiddenException();

        var count = await countReadService.GetByIdAsync(countId, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Cycle count {countId} not found.");

        var variances = new List<ICycleCountService.CountVariance>();
        var adjustmentDocuments = new List<InventoryDocument>();
        var transactions = new List<(
            Guid InventoryId,
            Guid ProductId,
            Guid VariantId,
            Guid WarehouseId,
            InventoryTransactionType Type,
            int Quantity,
            int BalanceAfter,
            string Reason)>();

        // Calculate variances for each counted item
        foreach (var countedItem in countedItems)
        {
            var inventory = await inventoryReadService.GetByVariationAndWarehouseAsync(
                countedItem.VariantId, count.WarehouseId, ct);

            if (inventory is null)
                continue;

            var expected = inventory.AvailableQuantity;
            var actual = countedItem.ActualQuantity;
            var variance = actual - expected;
            var variancePercent = expected > 0
                ? Math.Abs(variance) / (decimal)expected * 100m
                : 0m;

            variances.Add(new ICycleCountService.CountVariance(
                InventoryId: inventory.Id,
                VariantId: countedItem.VariantId,
                ExpectedQuantity: expected,
                ActualQuantity: actual,
                Variance: variance,
                VariancePercent: variancePercent));

            // Auto-adjust if variance exceeds threshold
            if (Math.Abs(variancePercent) > varianceThresholdPercent)
            {
                var adjusted = await inventoryWriteService.AdjustStockAsync(
                    inventory.Id, actual, ct);

                // Create adjustment document
                var adjustmentDoc = await documentService.CreateAndCompleteAsync(
                    type: InventoryDocumentType.Adjustment,
                    reason: InventoryDocumentReason.CycleCount,
                    sourceWarehouseId: count.WarehouseId,
                    destinationWarehouseId: null,
                    description: $"Cycle count variance: {expected} → {actual} ({variance:+#;-#;0})",
                    ct: ct);

                adjustmentDoc.AddItem(
                    productId: inventory.ProductId,
                    productVariantId: countedItem.VariantId,
                    quantity: Math.Abs(variance),
                    unitOfMeasure: "EA",
                    description: $"Cycle count adjustment");

                adjustmentDocuments.Add(adjustmentDoc);

                transactions.Add((
                    inventory.Id,
                    inventory.ProductId,
                    inventory.VariantId,
                    inventory.WarehouseId,
                    InventoryTransactionType.Adjustment,
                    variance,
                    adjusted.Entity.AvailableQuantity,
                    $"Cycle count variance: {variance:+#;-#;0}"));
            }
        }

        // Record all transactions
        if (transactions.Count > 0)
        {
            await transactionService.RecordBatchAsync(transactions, ct);
        }

        // Drive the count through its full workflow (Draft -> Counting -> PendingApproval ->
        // Approved -> Completed) in one update - this command is the single point where physical
        // counts are known, so it owns every transition rather than exposing each as its own API.
        await countWriteService.UpdateAsync(count.Id, c =>
        {
            foreach (var variance in variances)
                c.AddItem(variance.InventoryId, variance.VariantId, variance.ExpectedQuantity);

            c.StartCounting();

            foreach (var item in c.Items)
            {
                var actual = variances.First(v => v.InventoryId == item.InventoryId).ActualQuantity;
                c.RecordCount(item.Id, actual);
            }

            c.SubmitForApproval();
            c.Approve(approvedBy);
            c.Complete();
        }, ct);

        return new ICycleCountService.CycleCountResult(
            CountDocument: count,
            Variances: variances,
            AdjustmentDocuments: adjustmentDocuments,
            ItemsWithVariance: variances.Count(v => v.Variance != 0),
            ItemsAdjusted: adjustmentDocuments.Count);
    }

    private static string GenerateCountNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = Random.Shared.Next(1000, 9999);
        return $"CNT-{timestamp}-{random}";
    }
}
