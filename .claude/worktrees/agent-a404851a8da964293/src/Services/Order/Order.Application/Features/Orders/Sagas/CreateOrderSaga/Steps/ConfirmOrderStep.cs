using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Order;
using NovaCore.BuildingBlock.Saga.Abstractions;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;

namespace NovaCore.Order.Application.Features.Orders.Sagas.CreateOrderSaga.Steps;

/// <summary>
/// Saga Step 2 - moves the order Pending -> Confirmed and enqueues OrderConfirmedIntegrationEvent
/// in the same transaction/SaveChanges as the status change. Notification Service reacts to that
/// event asynchronously (persist UserNotification + realtime push) - see
/// docs/reference/create-order-saga.md for why notification/realtime aren't synchronous saga
/// steps here.
/// </summary>
public sealed class ConfirmOrderStep(
    IOrderWriteService orderWriteService,
    IOutboxStore outboxStore,
    IUnitOfWork uow,
    IAppLogger<ConfirmOrderStep> logger) : ISagaStep
{
    public const string Name = "ConfirmOrder";
    public string StepName => Name;

    public async Task ExecuteAsync(ISagaContext context, CancellationToken ct = default)
    {
        var orderId = context.Get<Guid>(CreateOrderSagaContextKeys.OrderId);
        var customerId = context.Get<Guid>(CreateOrderSagaContextKeys.CustomerId);

        await uow.ExecuteTransactionAsync(async () =>
        {
            var (tenantId, totalAmount) = await orderWriteService.ConfirmAsync(orderId, ct);

            await outboxStore.EnqueueAsync(new OrderConfirmedIntegrationEvent(tenantId, orderId, customerId, totalAmount), ct);
        },
        ct: ct);

        logger.Information("Order {OrderId} confirmed", orderId);
    }

    /// <summary>
    /// This is the saga's last step - SagaOrchestrator only compensates already-*completed*
    /// steps when a later one fails, and there is no step after this one, so this never actually
    /// runs. Kept only to satisfy ISagaStep.
    /// </summary>
    public Task CompensateAsync(ISagaContext context, CancellationToken ct = default) => Task.CompletedTask;
}
