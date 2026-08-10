using NovaCore.Order.Application.Abstractions.Persistence.ProductCatalogs;
using NovaCore.Order.Application.Abstractions.Services;
using NovaCore.Order.Application.Features.Orders.DTOs;

namespace NovaCore.Order.Application.Features.Stock;

public sealed class OrderItemPreparationService(
    IProductCatalogReadService catalogReadService,
    IInventoryClientService inventoryClient,
    ICartService cartService) : IOrderItemPreparationService
{
    public async Task<IReadOnlyList<PreparedOrderItem>> PrepareAsync(
        bool isAdminCreated,
        Guid ownerId,
        OrderItemRequestDto[] items,
        CancellationToken ct = default)
    {
        var (catalogs, resolvedItems) = isAdminCreated
            ? await EnsureItemsAreInStockAsync(items, ct)
            : await EnsureCartMatchesAsync(ownerId, items, ct);

        return resolvedItems
            .Select(item =>
            {
                var catalog = catalogs.FirstOrDefault(c => c.Id == item.VariationId)
                    ?? throw new NotFoundException(nameof(item.VariationId), item.VariationId);

                return new PreparedOrderItem(
                    item.ProductId,
                    item.VariationId,
                    catalog.ProductName,
                    catalog.VariationName,
                    catalog.Price.Value,
                    item.Quantity);
            })
            .ToArray();
    }

    private async Task<(ProductCatalog[] Catalogs, OrderItemRequestDto[] Items)> EnsureCartMatchesAsync(
        Guid customerId,
        OrderItemRequestDto[] items,
        CancellationToken ct)
    {
        var (catalogs, cart) = await cartService.GetCartAsync(customerId, ct);

        var requested = items.Select(i => (i.VariationId, i.Quantity)).ToHashSet();
        var current = cart.Items.Select(i => (i.VariationId, i.Quantity)).ToHashSet();

        if (!requested.SetEquals(current))
            throw new ConflictException(
                "Your cart has changed since it was last loaded (an item's quantity, availability, " +
                "or presence differs). Call GET /cart to refresh, then resubmit.");

        return (catalogs, cart.Items.Adapt<OrderItemRequestDto[]>());
    }

    private async Task<(ProductCatalog[] Catalogs, OrderItemRequestDto[] Items)> EnsureItemsAreInStockAsync(
        OrderItemRequestDto[] items,
        CancellationToken ct)
    {
        var variationIds = items
            .Select(i => i.VariationId)
            .ToArray();
        var catalogEntries = await catalogReadService.GetByVariantionIdsAsync(variationIds, ct);
        if (variationIds.Length != catalogEntries.Length)
            throw new ConflictException(
                "Your cart has changed since it was last loaded " +
                "(an item's quantity, availability, or presence differs).");

        var stockByVariation = await inventoryClient.GetAvailableStockBatchAsync(variationIds, ct);
        if (stockByVariation.Count != variationIds.Length)
            throw new ConflictException(
                "One or more items in your order are no longer available (deleted or deactivated).");

        if (stockByVariation.Values.Any(stock => stock < 1))
            throw new InsufficientAmountException("One or more items in your order are out of stock.");

        return (catalogEntries, items);
    }
}
