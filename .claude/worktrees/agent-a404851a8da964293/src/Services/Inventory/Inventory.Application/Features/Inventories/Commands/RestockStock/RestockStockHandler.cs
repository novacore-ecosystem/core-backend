using NovaCore.BuildingBlock.Application.Abstractions.Services;

using NovaCore.Inventory.Application.Abstractions.Persistence;
using NovaCore.Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using NovaCore.Inventory.Application.Abstractions.Persistence.Inventories;
using NovaCore.Inventory.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.RestockStock;

public sealed class RestockStockHandler(
    IInventoryDocumentReadService documentReadService,
    IInventoryReadService inventoryReadService,
    IInventoryWriteService inventoryWriteService,
    IInventoryTransactionService transactionService,
    OptimisticConcurrencyRetry concurrencyRetry,
    IUnitOfWork unitOfWork,
    IAppLogger<RestockStockHandler> logger) : ICommandHandler<RestockStockCommand, RestockStockResult>
{
    public async Task<RestockStockResult> Handle(RestockStockCommand request, CancellationToken ct = default)
    {
        var document = await documentReadService.GetByNumberAsync(request.DeductionId.ToString(), ct);

        if (document is null || document.Status != InventoryDocumentStatus.Completed)
        {
            logger.Information(
                "RestockStock {DeductionId} is a no-op ({Status})",
                request.DeductionId, document?.Status.ToString() ?? "NeverDeducted");
            return new RestockStockResult(true);
        }

        var reasonText = request.Reason is null ? "Deduction reversal" : $"Deduction reversal: {request.Reason}";

        await concurrencyRetry.ExecuteAsync(async (cancellationToken) =>
            await ReverseDeductionAsync(document, reasonText, cancellationToken), ct: ct);

        return new RestockStockResult(true);
    }

    private async Task ReverseDeductionAsync(InventoryDocument document, string reason, CancellationToken ct)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            foreach (var item in document.Items)
            {
                var inventory = await inventoryReadService.GetByIdAsync(item.InventoryId!.Value, ct);
                if (inventory is null)
                {
                    logger.Warning(
                        "RestockStock: no inventory row {InventoryId}, skipping",
                        item.InventoryId);
                    continue;
                }

                var inv = await inventoryWriteService.ReceiveStockAsync(inventory.Id, item.Quantity.Value, ct);

                await transactionService.RecordAsync(
                    inventoryId: inv.Id,
                    productId: inv.ProductId,
                    productVariantId: inv.VariantId,
                    warehouseId: inv.WarehouseId,
                    type: InventoryTransactionType.Receipt,
                    quantity: item.Quantity.Value,
                    balanceAfter: inv.AvailableQuantity,
                    reason: reason,
                    ct: ct);
            }

            logger.Information("Reversed deduction for order {OrderId}: {ItemCount} items", document.Number, document.Items.Count);
        }, ct: ct);
    }
}
