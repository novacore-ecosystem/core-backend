using NovaCore.BuildingBlock.Application.Abstractions.Services;

using NovaCore.Inventory.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.StockOut;

public sealed class StockOutHandler(
    IInventoryAdjustmentService adjustmentService,
    IUnitOfWork unitOfWork,
    IAppLogger<StockOutHandler> logger) : ICommandHandler<StockOutCommand, StockOutResponse>
{
    public async Task<StockOutResponse> Handle(StockOutCommand request, CancellationToken ct = default)
    {
        IInventoryAdjustmentService.AdjustmentResult? result = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            result = await adjustmentService.IssueAsync(
                request.InventoryId,
                request.Quantity,
                request.Reason.Trim(),
                ct);

            logger.Information(
                "Issued {Quantity} units from inventory {InventoryId}, remaining: {RemainingQuantity}",
                request.Quantity, request.InventoryId, result.Inventory.AvailableQuantity);
        }, ct: ct);

        return new StockOutResponse(result!.Inventory.AvailableQuantity);
    }
}
