using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Abstractions.Services;

/// <summary>
/// Warehouse transfer workflow: atomic two-way transfer.
/// Handles transferring stock between warehouses with complete audit trail.
/// </summary>
public interface ITransferService : IService
{
    Task<TransferResult> TransferAsync(
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        IReadOnlyList<TransferItem> items,
        string reason,
        CancellationToken ct = default);

    public sealed record TransferItem(
        Guid VariantId,
        int Quantity);

    public sealed record TransferResult(
        InventoryDocument SourceDocument,
        InventoryDocument DestinationDocument,
        IReadOnlyList<InventoryStock> SourceInventories,
        IReadOnlyList<InventoryStock> DestinationInventories);
}
