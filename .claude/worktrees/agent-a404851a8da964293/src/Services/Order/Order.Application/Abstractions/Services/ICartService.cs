namespace NovaCore.Order.Application.Abstractions.Services;

/// <summary>
/// Redis-backed per-user cart (see NovaCore.Order.Infrastructure.Caching.CartService). Every operation is
/// keyed by userId only - callers must resolve it from ICurrentUserService, never trust it from
/// a request body/query param.
/// </summary>
public interface ICartService
{
    Task<(ProductCatalog[] Catalogs, CartResponse Cart)> GetCartAsync(
        Guid userId,
        CancellationToken ct = default);

    /// <summary>Adds Quantity to the existing line if the variation is already in the cart, otherwise inserts a new line.</summary>
    Task<CartResponse> AddItemAsync(
        Guid userId,
        Guid variationId,
        int quantity,
        CancellationToken ct = default);

    /// <summary>Sets the line to an absolute Quantity. Rejects Quantity &lt;= 0 - callers should remove the line instead.</summary>
    Task<CartResponse> UpdateItemQuantityAsync(
        Guid userId,
        Guid variationId,
        int quantity,
        CancellationToken ct = default);

    Task<CartResponse> RemoveItemAsync(
        Guid userId,
        Guid variationId,
        CancellationToken ct = default);

    Task ClearCartAsync(Guid userId, CancellationToken ct = default);
}

/// <summary>AvailableStock/IsInsufficientStock are always live from Inventory (see IStockAvailabilityService) - never dropped from the response even when insufficient, so the client can mark/disable the specific line instead of silently losing it (unlike a deleted/unorderable variation, which IS pruned - see CartService.EnrichAndPruneAsync).</summary>
public sealed record CartItemResponse(
    Guid ProductId,
    Guid VariationId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    int AvailableStock,
    bool IsInsufficientStock);

public sealed record CartResponse(IReadOnlyCollection<CartItemResponse> Items)
{
    public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);
}
