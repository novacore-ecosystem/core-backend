namespace NovaCore.Inventory.Application.Features.Inventories.Commands.CycleCount;

public sealed record StartCycleCountCommand(
    Guid WarehouseId,
    DateTime CountDate,
    string Description) : ICommand<StartCycleCountResponse>;

public sealed record StartCycleCountResponse(
    Guid CountId,
    string CountNumber,
    string Status);
