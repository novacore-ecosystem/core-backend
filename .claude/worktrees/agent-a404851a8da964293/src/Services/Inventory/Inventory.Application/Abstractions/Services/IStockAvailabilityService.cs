using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Abstractions.Services;

public interface IStockAvailabilityService : IService
{
    Task<StockAvailabilityResult> ValidateAsync(
        IReadOnlyList<(Guid VariantId, int Quantity)> items,
        Guid warehouseId,
        CancellationToken ct = default);

    public sealed record StockAvailabilityResult(
        bool Success,
        IReadOnlyList<InventoryStock> AvailableInventories,
        IReadOnlyList<InsufficientStockError> InsufficientItems);

    public sealed record InsufficientStockError(
        Guid InventoryId,
        Guid VariantId,
        int RequestedQuantity,
        int AvailableQuantity);
}
