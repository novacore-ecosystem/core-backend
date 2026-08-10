using NovaCore.BuildingBlock.Application.Abstractions.Services;

using NovaCore.Inventory.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.StockIn;

public sealed class StockInHandler(
    IInventoryAdjustmentService adjustmentService,
    IUnitOfWork unitOfWork,
    IAppLogger<StockInHandler> logger) : ICommandHandler<StockInCommand, StockInResponse>
{
    public async Task<StockInResponse> Handle(StockInCommand request, CancellationToken ct = default)
    {
        IInventoryAdjustmentService.AdjustmentResult? result = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            result = await adjustmentService.ReceiveAsync(
                request.InventoryId,
                request.Quantity,
                request.Reason.Trim(),
                ct);

            logger.Information(
                "Received {Quantity} units into inventory {InventoryId}, new total: {NewQuantity}",
                request.Quantity, request.InventoryId, result.Inventory.AvailableQuantity);
        }, ct: ct);

        return new StockInResponse(result!.Inventory.AvailableQuantity);
    }
}
