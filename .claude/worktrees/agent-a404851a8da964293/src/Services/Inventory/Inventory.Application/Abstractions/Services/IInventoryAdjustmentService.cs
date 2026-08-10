using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Abstractions.Services;

/// <summary>
/// Inventory adjustment workflow: validates, adjusts, records, documents.
/// Handles three types of single-item adjustments: corrections, receipts, and issues.
/// </summary>
public interface IInventoryAdjustmentService : IService
{
    Task<AdjustmentResult> AdjustToAsync(
        Guid inventoryId,
        int newQuantity,
        string reason,
        CancellationToken ct = default);

    Task<AdjustmentResult> ReceiveAsync(
        Guid inventoryId,
        int quantity,
        string reason,
        CancellationToken ct = default);

    Task<AdjustmentResult> IssueAsync(
        Guid inventoryId,
        int quantity,
        string reason,
        CancellationToken ct = default);

    public sealed record AdjustmentResult(
        InventoryStock Inventory,
        int Delta,
        InventoryDocument Document);
}
