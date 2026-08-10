namespace NovaCore.Inventory.Application.Features.Inventories.Commands.RestockStock;

/// <summary>
/// Compensating action for a previously successful DeductStock call, looked up by the same
/// DeductionId - CreateOrderSaga's DeductInventoryStep.CompensateAsync calls this when a later
/// saga step fails after inventory was already deducted.
/// </summary>
public sealed record RestockStockCommand(Guid DeductionId, string? Reason = null) : ICommand<RestockStockResult>;

public sealed record RestockStockResult(bool Success);
