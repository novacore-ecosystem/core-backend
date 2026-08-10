using NovaCore.Order.Application.Abstractions.Services;
using NovaCore.Order.Application.Features.Orders.DTOs;

namespace NovaCore.Order.Application.Abstractions.Persistence.Orders;

public interface IOrderWriteService
{
    /// <summary>
    /// Returns the created OrderEntity - CreateOrderHandler needs the whole aggregate (Id,
    /// OrderNumber, Items, GrandTotal) to build OrderCreatedIntegrationEvent, so returning a
    /// narrower DTO here would just be a redundant projection of the same data.
    /// </summary>
    Task<OrderEntity> CreateAsync(CreateOrderRequest request, CancellationToken ct = default);

    /// <summary>Updates the customer-editable shipping/contact snapshot (Order.UpdateOwnerInfo enforces the non-terminal-status guard).</summary>
    Task UpdateOwnerInfoAsync(
        Guid orderId,
        string ownerName,
        Email ownerEmail,
        PhoneNumber ownerPhone,
        string idempotencyKey,
        CancellationToken ct = default);

    Task<(Guid TenantId, decimal TotalAmount)> ConfirmAsync(Guid orderId, CancellationToken ct = default);

    Task<(Guid TenantId, Guid CustomerId)> CancelAsync(Guid orderId, string reason, CancellationToken ct = default);

    Task<Guid> CompleteAsync(Guid orderId, CancellationToken ct = default);

    Task DeleteAsync(Guid orderId, CancellationToken ct = default);
}
