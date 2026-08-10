using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Stock;

public sealed class StockAvailabilityService(IInventoryClientService inventoryClient) : IStockAvailabilityService
{
    public async Task<IReadOnlyDictionary<Guid, StockAvailability>> CheckAsync(
        IReadOnlyCollection<StockRequest> requests,
        CancellationToken ct = default)
    {
        var variationIds = requests.Select(r => r.VariationId).Distinct().ToArray();
        var stockByVariation = await inventoryClient.GetAvailableStockBatchAsync(variationIds, ct);

        return requests.ToDictionary(
            r => r.VariationId,
            r => new StockAvailability(r.VariationId, r.RequestedQuantity, stockByVariation.GetValueOrDefault(r.VariationId)));
    }
}
