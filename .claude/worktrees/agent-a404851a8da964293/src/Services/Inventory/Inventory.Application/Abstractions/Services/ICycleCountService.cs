using NovaCore.BuildingBlock.Application.Abstractions.Services;

namespace NovaCore.Inventory.Application.Abstractions.Services;

/// <summary>
/// Cycle count workflow: variance calculation and auto-adjustment.
/// Manages physical inventory counting with automatic reconciliation and variance reporting.
/// </summary>
public interface ICycleCountService : IService
{
    Task<InventoryCount> StartCountAsync(
        Guid warehouseId,
        DateTime countDate,
        string description,
        CancellationToken ct = default);

    Task<CycleCountResult> CompleteCountAsync(
        Guid countId,
        IReadOnlyList<CountItem> countedItems,
        decimal varianceThresholdPercent = 5m,
        CancellationToken ct = default);

    public sealed record CountItem(
        Guid VariantId,
        int ActualQuantity);

    public sealed record CountVariance(
        Guid InventoryId,
        Guid VariantId,
        int ExpectedQuantity,
        int ActualQuantity,
        int Variance,
        decimal VariancePercent);

    public sealed record CycleCountResult(
        InventoryCount CountDocument,
        IReadOnlyList<CountVariance> Variances,
        IReadOnlyList<InventoryDocument> AdjustmentDocuments,
        int ItemsWithVariance,
        int ItemsAdjusted);
}
