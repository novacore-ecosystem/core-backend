using NovaCore.BuildingBlock.Application.Abstractions.Services;

using NovaCore.Inventory.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.AdjustStock;

public sealed class AdjustStockHandler(
    IInventoryAdjustmentService adjustmentService,
    IUnitOfWork unitOfWork,
    IAppLogger<AdjustStockHandler> logger) : ICommandHandler<AdjustStockCommand, AdjustStockResponse>
{
    public async Task<AdjustStockResponse> Handle(AdjustStockCommand request, CancellationToken ct = default)
    {
        IInventoryAdjustmentService.AdjustmentResult? result = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            result = await adjustmentService.AdjustToAsync(
                request.InventoryId,
                request.NewQuantity,
                request.Reason.Trim(),
                ct);

            logger.Information(
                "Adjusted inventory {InventoryId} to {NewQuantity} (delta: {Delta})",
                request.InventoryId, request.NewQuantity, result.Delta);
        }, ct: ct);

        return new AdjustStockResponse(result!.Inventory.AvailableQuantity);
    }
}
