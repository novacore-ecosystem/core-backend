using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Contract.Events.Order;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;

namespace NovaCore.Order.Application.Features.Orders.Commands.CompleteOrder;

public sealed class CompleteOrderHandler(
    IOrderWriteService orderWriteService,
    IOutboxStore outboxStore,
    IUnitOfWork uow) : ICommandHandler<CompleteOrderCommand, CompleteOrderResponse>
{
    public async Task<CompleteOrderResponse> Handle(CompleteOrderCommand request, CancellationToken ct = default)
    {
        await uow.ExecuteTransactionAsync(async () =>
        {
            var customerId = await orderWriteService.CompleteAsync(request.OrderId, ct);

            var orderCompletedEvent = new OrderCompletedIntegrationEvent(request.OrderId, customerId);
            await outboxStore.EnqueueAsync(orderCompletedEvent, ct);
        }, ct: ct);

        return new CompleteOrderResponse(request.OrderId, OrderStatus.Completed);
    }
}
