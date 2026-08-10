using NovaCore.BuildingBlock.Application.Abstractions.Services;

using NovaCore.Inventory.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Features.Inventories.Commands.CycleCount;

public sealed class StartCycleCountHandler(
    ICycleCountService cycleCountService,
    IUnitOfWork unitOfWork,
    IAppLogger<StartCycleCountHandler> logger) : ICommandHandler<StartCycleCountCommand, StartCycleCountResponse>
{
    public async Task<StartCycleCountResponse> Handle(StartCycleCountCommand request, CancellationToken ct = default)
    {
        InventoryCount? count = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            count = await cycleCountService.StartCountAsync(
                request.WarehouseId,
                request.CountDate,
                request.Description,
                ct);

            logger.Information(
                "Started cycle count {CountNumber} in warehouse {WarehouseId}",
                count.Number,
                request.WarehouseId);
        }, ct: ct);

        return new StartCycleCountResponse(
            CountId: count!.Id,
            CountNumber: count.Number,
            Status: count.Status.ToString());
    }
}
