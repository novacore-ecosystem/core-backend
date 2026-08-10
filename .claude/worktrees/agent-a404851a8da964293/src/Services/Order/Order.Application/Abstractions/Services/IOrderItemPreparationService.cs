using NovaCore.Order.Application.Features.Orders.DTOs;

namespace NovaCore.Order.Application.Abstractions.Services;

/// <summary>
/// Validates+resolves the items on a CreateOrder request into fully catalog-priced lines, before
/// Order.Create ever runs - a client-created order must match the caller's current cart exactly
/// (EnsureCartMatchesAsync); an admin-created order only needs its items to exist and be in stock
/// (EnsureItemsAreInStockAsync). Either way, callers get back ProductCatalog-resolved
/// name/price data so CreateOrderRequest never has to look anything up itself.
/// </summary>
public interface IOrderItemPreparationService
{
    Task<IReadOnlyList<PreparedOrderItem>> PrepareAsync(
        bool isAdminCreated,
        Guid ownerId,
        OrderItemRequestDto[] items,
        CancellationToken ct = default);
}
