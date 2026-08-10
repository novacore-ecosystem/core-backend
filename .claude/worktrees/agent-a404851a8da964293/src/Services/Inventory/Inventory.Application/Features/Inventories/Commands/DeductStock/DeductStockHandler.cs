using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Domain.Exceptions;

using NovaCore.Inventory.Application.Abstractions.Persistence;
using NovaCore.Inventory.Application.Abstractions.Persistence.Warehouses;
using NovaCore.Inventory.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.DeductStock;

public sealed class DeductStockHandler(
    IWarehouseReadService warehouseReadService,
    IInventoryDocumentReadService documentReadService,
    IStockAvailabilityService availabilityService,
    IInventoryDocumentService documentService,
    IStockDeductionService deductionService,
    OptimisticConcurrencyRetry concurrencyRetry,
    IUnitOfWork unitOfWork,
    IAppLogger<DeductStockHandler> logger) : ICommandHandler<DeductStockCommand, DeductStockResult>
{
    private const string MainWarehouseCode = "PLATFORM";

    public async Task<DeductStockResult> Handle(DeductStockCommand request, CancellationToken ct = default)
    {
        var existing = await documentReadService.GetByNumberAsync(request.DeductionId.ToString(), ct);
        if (existing is not null)
        {
            logger.Information(
                "Stock deduction {DeductionId} already processed ({Status}), replaying result",
                request.DeductionId, existing.Status);
            return ToResult(existing);
        }

        var warehouse = await warehouseReadService.GetByCodeAsync(MainWarehouseCode, ct)
            ?? throw ExceptionFactory.EntityNotFound($"Default platform warehouse '{MainWarehouseCode}' is not configured.");

        return await concurrencyRetry.ExecuteAsync(async (cancellationToken) =>
            await ProcessDeductionAsync(request, warehouse.Id, cancellationToken), ct: ct);
    }

    private async Task<DeductStockResult> ProcessDeductionAsync(
        DeductStockCommand request,
        Guid warehouseId,
        CancellationToken ct)
    {
        var reasonText = request.Reason is null ? "Order deduction" : $"Order deduction: {request.Reason}";
        var items = request.Items.Select(i => (i.VariantId, i.Quantity)).ToList();

        var validation = await availabilityService.ValidateAsync(items, warehouseId, ct);

        if (!validation.Success)
        {
            await unitOfWork.ExecuteTransactionAsync(async () =>
            {
                await documentService.CreateAndCompleteAsync(
                    number: request.DeductionId.ToString(),
                    type: InventoryDocumentType.Issue,
                    reason: InventoryDocumentReason.Sale,
                    sourceWarehouseId: warehouseId,
                    destinationWarehouseId: null,
                    description: $"FAILED: {reasonText}",
                    ct: ct);
            }, ct: ct);

            return new DeductStockResult(
                false,
                "InsufficientStock",
                [.. validation.InsufficientItems
                    .Select(i => new InsufficientStockItem(
                        i.VariantId,
                        i.RequestedQuantity,
                        i.AvailableQuantity))]);
        }

        DeductStockResult result = null!;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            var deductionItems = request.Items
                .Select(ri => (ri.VariantId, ri.Quantity))
                .Zip(validation.AvailableInventories)
                .Select(pair => (
                    pair.Second.Id,
                    pair.Second.ProductId,
                    pair.Second.VariantId,
                    pair.First.Quantity))
                .ToList();

            await deductionService.DeductAsync(
                documentNumber: request.DeductionId.ToString(),
                documentType: InventoryDocumentType.Issue,
                documentReason: InventoryDocumentReason.Sale,
                sourceWarehouseId: warehouseId,
                items: deductionItems,
                description: reasonText,
                ct: ct);

            result = new DeductStockResult(true, null, []);
            logger.Information(
                "Stock deducted for order {OrderId}: {ItemCount} items",
                request.DeductionId, request.Items.Count);
        }, ct: ct);

        return result;
    }

    private static DeductStockResult ToResult(InventoryDocument document) => document.Status switch
    {
        InventoryDocumentStatus.Completed => new DeductStockResult(true, null, []),
        InventoryDocumentStatus.Cancelled => new DeductStockResult(false, "InsufficientStock", []),
        _ => new DeductStockResult(false, "ProcessingFailed", []),
    };
}
