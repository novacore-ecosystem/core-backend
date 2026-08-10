using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.Exceptions;
using NovaCore.BuildingBlock.SharedKernel.Constants;

using NovaCore.Order.Application.Abstractions.Persistence.ProductCatalogs;
using NovaCore.Order.Application.Abstractions.Services;
using NovaCore.Order.Domain.Entities.Catalogs;

namespace NovaCore.Order.Infrastructure.Caching;

/// <summary>
/// Redis-backed cart, keyed per user. Only {VariationId, Quantity} pairs are stored - name/price/
/// orderable-status are always resolved live from ProductCatalog on every read or mutation,
/// the same "never trust/store stale price" principle OrderItemPreparationService already applies
/// to order items (see docs/services/order-service.md). A variation that's been deleted or
/// deactivated since being added is dropped from the cart the next time it's read, rather than
/// silently kept.
/// </summary>
public sealed class CartService(
    ICacheService cacheService,
    IProductCatalogReadService catalogReadService,
    IStockAvailabilityService stockAvailabilityService) : ICartService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(CacheKeyConstant.Cart.DefaultTtlMinutes);

    public async Task<(ProductCatalog[] Catalogs, CartResponse Cart)> GetCartAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var data = await LoadAsync(userId, ct);
        return await EnrichAndPruneAsync(userId, data, ct);
    }

    public async Task<CartResponse> AddItemAsync(
        Guid userId,
        Guid variationId,
        int quantity,
        CancellationToken ct = default)
    {
        var data = await LoadAsync(userId, ct);
        var existing = data.Items.FirstOrDefault(i => i.VariationId == variationId);
        var resultingQuantity = (existing?.Quantity ?? 0) + quantity;

        await EnsureOrderableAndInStockAsync(variationId, resultingQuantity, ct);

        if (existing is not null)
        {
            data.Items.Remove(existing);
            data.Items.Add(existing with { Quantity = resultingQuantity });
        }
        else
        {
            data.Items.Add(new CartLine(variationId, quantity));
        }

        await SaveAsync(userId, data, ct);
        var (_, cart) = await EnrichAndPruneAsync(userId, data, ct);
        return cart;
    }

    public async Task<CartResponse> UpdateItemQuantityAsync(
        Guid userId,
        Guid variationId,
        int quantity,
        CancellationToken ct = default)
    {
        if (quantity <= 0)
            throw ExceptionFactory.InvalidRange("Quantity must be greater than 0 - remove the item instead of setting it to zero or less.");

        var data = await LoadAsync(userId, ct);
        var existing = data.Items.FirstOrDefault(i => i.VariationId == variationId)
            ?? throw new NotFoundException("Cart item", variationId);

        await EnsureOrderableAndInStockAsync(variationId, quantity, ct);

        data.Items.Remove(existing);
        data.Items.Add(existing with { Quantity = quantity });

        await SaveAsync(userId, data, ct);
        var (_, cart) = await EnrichAndPruneAsync(userId, data, ct);
        return cart;
    }

    public async Task<CartResponse> RemoveItemAsync(
        Guid userId,
        Guid variationId,
        CancellationToken ct = default)
    {
        var data = await LoadAsync(userId, ct);
        data.Items.RemoveAll(i => i.VariationId == variationId);

        await SaveAsync(userId, data, ct);
        var (_, cart) = await EnrichAndPruneAsync(userId, data, ct);
        return cart;
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken ct = default)
    {
        await cacheService.RemoveAsync(CacheKeyConstant.Cart.Key(userId), ct);
    }

    /// <summary>Checked on every add/update-quantity - resultingQuantity is the line's total quantity AFTER this mutation (existing + delta for Add, the new absolute value for Update), never just the delta, so stock is validated against what the cart would actually hold.</summary>
    private async Task EnsureOrderableAndInStockAsync(
        Guid variationId,
        int resultingQuantity,
        CancellationToken ct)
    {
        var variations = await catalogReadService.GetByVariantionIdsAsync([variationId], ct);
        var variation = variations.FirstOrDefault()
            ?? throw new NotFoundException("Variation", variationId);

        if (!variation.IsOrderable)
            throw ExceptionFactory.InvalidState($"Product ({variation.ProductName}) is not currently available for ordering.");

        var availability = await stockAvailabilityService.CheckAsync(
            [new StockRequest(variationId, resultingQuantity)], ct);

        if (availability.TryGetValue(variationId, out var stock) && !stock.IsSufficient)
        {
            throw ExceptionFactory.InsufficientStock(
                $"Insufficient stock for Variation {variationId} (requested {stock.RequestedQuantity}, available {stock.AvailableQuantity})",
                detail: new { insufficients = new[] { variationId } });
        }
    }

    private async Task<CartData> LoadAsync(Guid userId, CancellationToken ct)
    {
        var data = await cacheService.GetAsync<CartData>(CacheKeyConstant.Cart.Key(userId), ct);
        return data ?? new CartData([]);
    }

    private async Task SaveAsync(Guid userId, CartData data, CancellationToken ct)
    {
        await cacheService.SetAsync(CacheKeyConstant.Cart.Key(userId), data, Ttl, ct);
    }

    /// <summary>
    /// Resolves live product/price data for every cart line, drops any line whose variation no
    /// longer exists or isn't orderable, and persists the pruned result. Insufficient STOCK is
    /// deliberately not pruned here (unlike a deleted/unorderable variation) - every remaining
    /// line gets a live AvailableStock/IsInsufficientStock so the client can mark/disable it,
    /// per the "surface it, don't silently drop it" requirement.
    /// </summary>
    private async Task<(ProductCatalog[] Catalogs, CartResponse Cart)> EnrichAndPruneAsync(
        Guid userId,
        CartData data,
        CancellationToken ct)
    {
        if (data.Items.Count == 0)
            return ([], new CartResponse([]));

        var variationIds = data.Items.Select(i => i.VariationId).ToArray();
        var catalogEntries = await catalogReadService.GetByVariantionIdsAsync(variationIds, ct);
        var catalogById = catalogEntries.ToDictionary(c => c.Id);

        var validItems = data.Items
            .Where(i => catalogById.TryGetValue(i.VariationId, out var entry) && entry.IsOrderable)
            .ToList();

        if (validItems.Count != data.Items.Count)
            await SaveAsync(userId, new CartData(validItems), ct);

        if (validItems.Count == 0)
            return ([], new CartResponse([]));

        var stockRequests = validItems
            .Select(i => new StockRequest(i.VariationId, i.Quantity))
            .ToArray();
        var availability = await stockAvailabilityService.CheckAsync(stockRequests, ct);

        var cart = new CartResponse([.. validItems
            .Select(i =>
            {
                var entry = catalogById[i.VariationId];
                var stock = availability[i.VariationId];
                return new CartItemResponse(
                    entry.ProductId,
                    i.VariationId,
                    entry.ProductName,
                    entry.Price.Value,
                    i.Quantity,
                    stock.AvailableQuantity,
                    !stock.IsSufficient);
            })]);
        return (catalogEntries, cart);
    }
}

internal sealed record CartData(List<CartLine> Items);

internal sealed record CartLine(Guid VariationId, int Quantity);
