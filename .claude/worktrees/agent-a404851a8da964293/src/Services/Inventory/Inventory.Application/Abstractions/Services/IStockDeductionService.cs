using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Abstractions.Services;

/// <summary>
/// Stock deduction workflow: validates, deducts, records, documents.
/// Coordinates the complete process of removing stock from inventory for fulfillment.
/// </summary>
public interface IStockDeductionService : IService
{
    Task<StockDeductionResult> DeductAsync(
        string documentNumber,
        InventoryDocumentType documentType,
        InventoryDocumentReason documentReason,
        Guid sourceWarehouseId,
        IReadOnlyList<(Guid InventoryId, Guid ProductId, Guid VariantId, int Quantity)> items,
        string description,
        CancellationToken ct = default);

    public sealed record StockDeductionResult(
        bool Success,
        InventoryDocument Document,
        IReadOnlyList<InventoryStock> DeductedInventories);
}
