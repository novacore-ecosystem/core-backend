using NovaCore.BuildingBlock.Contract.Protos.Inventory;

using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Infrastructure.GrpcClients;

/// <summary>Thin adapter over InventoryGrpcService - maps Order's own Application-layer DTOs to/from the generated proto types, no business logic here (see docs/services/inventory-service.md).</summary>
public sealed class InventoryClientService(InventoryGrpcService.InventoryGrpcServiceClient client) : IInventoryClientService
{
    public async Task<IReadOnlyDictionary<Guid, int>> GetAvailableStockBatchAsync(IReadOnlyCollection<Guid> productVariationIds, CancellationToken ct = default)
    {
        var request = new GetProductsStockRequest();
        request.VariantIds.AddRange(productVariationIds.Select(id => id.ToString()));

        var response = await client.GetProductsStockAsync(request, cancellationToken: ct);

        return response.Items.ToDictionary(
            i => Guid.Parse(i.VariantId),
            i => i.TotalQuantity);
    }

    public async Task<InventoryDeductionResult> DeductStockAsync(
        Guid deductionId,
        IReadOnlyCollection<InventoryDeductionItem> items,
        string? reason,
        CancellationToken ct = default)
    {
        var request = new DeductStockRequest
        {
            DeductionId = deductionId.ToString(),
            Reason = reason ?? string.Empty,
        };

        request.Items.AddRange(items.Select(i => new DeductStockItem
        {
            VariantId = i.VariantId.ToString(),
            Quantity = i.Quantity,
        }));

        var response = await client.DeductStockAsync(request, cancellationToken: ct);

        var insufficientItems = response.InsufficientItems
            .Select(i => new InventoryDeductionInsufficientItem(
                Guid.Parse(i.VariantId), i.RequestedQuantity, i.AvailableQuantity))
            .ToList();

        return new InventoryDeductionResult(
            response.Success,
            string.IsNullOrEmpty(response.FailureCode) ? null : response.FailureCode,
            insufficientItems);
    }

    public async Task RestockAsync(Guid deductionId, string? reason, CancellationToken ct = default)
    {
        var request = new RestockStockRequest
        {
            DeductionId = deductionId.ToString(),
            Reason = reason ?? string.Empty,
        };

        await client.RestockStockAsync(request, cancellationToken: ct);
    }
}
