using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events.Order;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderHandler(
    IOrderWriteService orderWriteService,
    IOutboxStore outboxStore,
    IInventoryClientService inventoryClient,
    IUnitOfWork uow) : ICommandHandler<CancelOrderCommand, CancelOrderResponse>
{
    public async Task<CancelOrderResponse> Handle(CancelOrderCommand request, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(async () =>
        {
            var (tenantId, customerId) = await orderWriteService.CancelAsync(request.OrderId, request.Reason, ct);

            var orderCancelledEvent = new OrderCancelledIntegrationEvent(
                tenantId,
                request.OrderId,
                customerId,
                request.Reason);
            await outboxStore.EnqueueAsync(orderCancelledEvent, ct);
        }, ct: ct);

        // Deduction is keyed by OrderId and idempotent (StockDeduction ledger in Inventory), so
        // this is safe to call unconditionally even if the order was still Pending and
        // DeductInventoryStep hadn't run yet - it's a no-op when there's nothing to reverse.
        await inventoryClient.RestockAsync(request.OrderId, reason: request.Reason ?? "Order cancelled", ct);

        return new CancelOrderResponse(request.OrderId, OrderStatus.Cancelled);
    }
}
