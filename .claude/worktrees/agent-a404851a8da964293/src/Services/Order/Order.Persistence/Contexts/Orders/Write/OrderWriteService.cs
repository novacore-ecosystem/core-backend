using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Domain.Enums;
using NovaCore.BuildingBlock.Domain.ValueObjects;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Features.Orders.DTOs;
using NovaCore.Order.Domain.Enums;
using NovaCore.Order.Domain.ValueObjects;
using NovaCore.Order.Persistence.Contexts.Orders.Repositories;
using NovaCore.Order.Persistence.Contexts.OrderStatusHistories.Repositories;
using NovaCore.Order.Persistence.Mapping.Orders;

namespace NovaCore.Order.Persistence.Contexts.Orders.Write;

public sealed class OrderWriteService(
    IOrderRepository orderRepo,
    IOrderStatusHistoryRepository statusHistoryRepo,
    ICurrentUserService currentUser) : IOrderWriteService
{
    public async Task<OrderEntity> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        var order = OrderEntity.Create(OrderMapper.ToCreateOrderData(request));

        await orderRepo.AddAsync(order, ct);
        await statusHistoryRepo.AddAsync(
            OrderStatusHistory.Record(
                order.Id,
                OrderStatusHistoryType.Created,
                previousStatus: null,
                currentStatus: order.Status,
                currentUser.GetUserId(),
                currentUser.GetUserName()),
            ct);

        return order;
    }

    public async Task UpdateOwnerInfoAsync(
        Guid orderId,
        string ownerName,
        Email ownerEmail,
        PhoneNumber ownerPhone,
        string idempotencyKey,
        CancellationToken ct = default)
    {
        await orderRepo.UpdateAsync(
            id: orderId,
            updateAction: order =>
            {
                if (order.Status is not OrderStatus.Confirmed)
                    throw new BadRequestException(MessageCode.InvalidOrderStatus);

                order.UpdateOwnerInfo(ownerName, ownerEmail, ownerPhone, idempotencyKey);
            },
            ct);
    }

    public async Task<(Guid TenantId, decimal TotalAmount)> ConfirmAsync(Guid orderId, CancellationToken ct = default)
    {
        var tenantId = Guid.Empty;
        var totalAmount = 0m;
        var previousStatus = OrderStatus.Pending;

        await orderRepo.UpdateAsync(
            orderId,
            includes: query => query.Include(o => o.Price),
            updateAction: order =>
            {
                previousStatus = order.Status;
                order.Accept();
                tenantId = order.TenantId;
                totalAmount = order.GrandTotal.Value;
            },
            ct);

        await RecordStatusHistoryAsync(orderId, OrderStatusHistoryType.Confirmed, previousStatus, OrderStatus.Confirmed, ct: ct);

        return (tenantId, totalAmount);
    }

    public async Task<(Guid TenantId, Guid CustomerId)> CancelAsync(Guid orderId, string reason, CancellationToken ct = default)
    {
        var tenantId = Guid.Empty;
        var customerId = Guid.Empty;
        var previousStatus = OrderStatus.Pending;

        await orderRepo.UpdateAsync(
            orderId,
            includes: query => query.Include(o => o.Owner).Include(o => o.Shipping).Include(o => o.Payment),
            updateAction: order =>
            {
                if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed))
                    throw new BadRequestException(MessageCode.InvalidOrderStatus);

                previousStatus = order.Status;
                order.Cancel(
                    reason,
                    currentUser.GetUserId(),
                    currentUser.IsAuthenticated() ? currentUser.GetUserName() : null);
                tenantId = order.TenantId;
                customerId = order.Owner.OwnerId;
            },
            ct);

        await RecordStatusHistoryAsync(orderId, OrderStatusHistoryType.Cancelled, previousStatus, OrderStatus.Cancelled, reason, ct: ct);

        return (tenantId, customerId);
    }

    public async Task<Guid> CompleteAsync(Guid orderId, CancellationToken ct = default)
    {
        var customerId = Guid.Empty;
        var previousStatus = OrderStatus.Processing;

        await orderRepo.UpdateAsync(
            orderId,
            includes: query => query.Include(o => o.Owner).Include(o => o.Shipping),
            updateAction: order =>
            {
                if (order.Status is not OrderStatus.Confirmed)
                    throw new BadRequestException(MessageCode.InvalidOrderStatus);

                previousStatus = order.Status;
                order.Complete();
                customerId = order.Owner.OwnerId;
            },
            ct);

        await RecordStatusHistoryAsync(orderId, OrderStatusHistoryType.Delivered, previousStatus, OrderStatus.Completed, ct: ct);

        return customerId;
    }

    /// <summary>Actor is whoever is resolvable from the ambient request context - null for saga-driven transitions with no HTTP caller (e.g. ConfirmOrderStep).</summary>
    private async Task RecordStatusHistoryAsync(
        Guid orderId,
        OrderStatusHistoryType eventType,
        OrderStatus previousStatus,
        OrderStatus currentStatus,
        string? reason = null,
        CancellationToken ct = default)
    {
        await statusHistoryRepo.AddAsync(
            OrderStatusHistory.Record(
                orderId,
                eventType,
                previousStatus,
                currentStatus,
                currentUser.GetUserId(),
                currentUser.IsAuthenticated() ? currentUser.GetUserName() : null,
                reason),
            ct);
    }

    public async Task DeleteAsync(Guid orderId, CancellationToken ct = default)
    {
        await orderRepo.DeleteByIdAsync(orderId, ct);
    }
}
