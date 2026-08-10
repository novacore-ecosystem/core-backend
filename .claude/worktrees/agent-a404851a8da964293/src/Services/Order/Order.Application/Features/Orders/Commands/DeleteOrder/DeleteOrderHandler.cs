using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Exceptions;
using NovaCore.BuildingBlock.Contract.Events.Order;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Orders.Commands.DeleteOrder;

/// <summary>
/// Hard delete (matches the codebase's convention - no soft-delete flag on BaseEntity). Only
/// Pending or Cancelled orders may be deleted. A Pending order can already have had stock
/// deducted - CreateOrderSaga's DeductInventoryStep runs before ConfirmOrderStep, so there's a
/// window where Status is still Pending but the deduction already succeeded - and a Cancelled
/// order was already restocked by CancelOrderHandler. RestockAsync is called unconditionally
/// before the delete either way: it's keyed by OrderId and idempotent in Inventory's
/// StockDeduction ledger, so it's a no-op when there's nothing left to reverse.
/// </summary>
public sealed class DeleteOrderHandler(
    IOrderReadService orderReadService,
    IOrderWriteService orderWriteService,
    IOutboxStore outboxStore,
    IInventoryClientService inventoryClient,
    IUnitOfWork uow) : ICommandHandler<DeleteOrderCommand, DeleteOrderResponse>
{
    public async Task<DeleteOrderResponse> Handle(DeleteOrderCommand request, CancellationToken ct = default)
    {
        var order = await orderReadService.GetByIdAsync(request.OrderId, ct)
            ?? throw new NotFoundException("Order", request.OrderId);

        if (order.Status is not (OrderStatus.Pending or OrderStatus.Cancelled))
            throw new BadRequestException(MessageCode.InvalidOrderStatus);

        await inventoryClient.RestockAsync(request.OrderId, reason: "Order deleted", ct);

        await uow.ExecuteTransactionAsync(async () =>
        {
            await orderWriteService.DeleteAsync(request.OrderId, ct);

            await outboxStore.EnqueueAsync(
                new OrderDeletedIntegrationEvent(request.OrderId, order.Owner.OwnerId), ct);
        }, ct: ct);

        return new DeleteOrderResponse();
    }
}
