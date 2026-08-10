using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Abstractions.Services;

/// <summary>
/// Receiving workflow: multi-item receiving with lot tracking.
/// Handles purchase receipt operations with optional lot allocation for lot-tracked items.
/// </summary>
public interface IReceivingService : IService
{
    Task<ReceivingResult> ReceiveAsync(
        string purchaseOrderNumber,
        Guid warehouseId,
        IReadOnlyList<ReceivingItem> items,
        string description,
        CancellationToken ct = default);

    public sealed record ReceivingItem(
        Guid VariantId,
        Guid WarehouseId,
        int Quantity,
        string? LotNumber = null,
        DateTime? ManufactureDate = null,
        DateTime? ExpiryDate = null);

    public sealed record ReceivingResult(
        InventoryDocument Document,
        IReadOnlyList<InventoryStock> ReceivedInventories,
        IReadOnlyList<InventoryLot> CreatedLots);
}
