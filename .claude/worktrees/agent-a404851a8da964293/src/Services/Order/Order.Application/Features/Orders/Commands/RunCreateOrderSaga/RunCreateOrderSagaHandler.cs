using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Contract.Events.Order;
using NovaCore.BuildingBlock.Saga.Abstractions;
using NovaCore.BuildingBlock.Saga.Core;

using NovaCore.Order.Application.Abstractions.Persistence.Orders;
using NovaCore.Order.Application.Features.Orders.Sagas.CreateOrderSaga;
using NovaCore.Order.Application.Features.Orders.Sagas.CreateOrderSaga.Steps;

namespace NovaCore.Order.Application.Features.Orders.Commands.RunCreateOrderSaga;

/// <summary>
/// Drives CreateOrderSaga for one order (PHASE 5/6 of the CreateOrder workflow - see
/// docs/reference/create-order-saga.md). Dispatched via MediatR from OrderCreatedSagaConsumer
/// rather than run inline in the consumer, so the saga's dependencies (repos, gRPC client,
/// DbContext-backed IUnitOfWork) are resolved lazily when this command actually runs, not eagerly
/// when AddKafkaMessaging's DiscoverConsumerTopics constructs every registered consumer just to
/// read its Topics - keeping consumers themselves thin/side-effect-free at construction time,
/// same as every other consumer in this codebase.
/// </summary>
public sealed class RunCreateOrderSagaHandler(
    ISagaOrchestrator orchestrator,
    DeductInventoryStep deductInventoryStep,
    ConfirmOrderStep confirmOrderStep,
    IOrderWriteService orderWriteService,
    IOutboxStore outboxStore,
    IUnitOfWork uow,
    IAppLogger<RunCreateOrderSagaHandler> logger) : ICommandHandler<RunCreateOrderSagaCommand>
{
    public async Task Handle(RunCreateOrderSagaCommand request, CancellationToken ct = default)
    {
        var definition = CreateOrderSagaDefinitionFactory.Create(deductInventoryStep, confirmOrderStep);

        // SagaId = OrderId: one saga execution per order, and a redelivered/retried message for
        // the same order reuses (and overwrites) the same saga history row instead of creating a
        // new one - see EfSagaStore.
        var context = new SagaContext(
            sagaId: request.OrderId.ToString(),
            correlationId: request.CorrelationId);

        context.Set(CreateOrderSagaContextKeys.OrderId, request.OrderId);
        context.Set(CreateOrderSagaContextKeys.CustomerId, request.CustomerId);
        context.Set(CreateOrderSagaContextKeys.Items, request.Items);

        try
        {
            await orchestrator.ExecuteAsync(definition, context, ct);
            logger.Information("CreateOrderSaga completed for order {OrderId}", request.OrderId);
        }
        catch (SagaExecutionException ex) when (ex.FailedStepName == DeductInventoryStep.Name)
        {
            // Terminal business outcome, not a system failure - cancel the order and stop. No
            // compensation runs (nothing completed before Step 1), matching PHASE 6's "saga ends".
            var reason = context.Get<string>(CreateOrderSagaContextKeys.FailureReason) ?? "OutOfStock";
            await CancelOrderAsync(request.OrderId, request.CustomerId, reason, ct);
        }
        catch (SagaExecutionException ex) when (ex.InnerException is InvalidStatusException statusEx)
        {
            // The order already left Pending status by the time ConfirmOrder ran (e.g. a manual
            // CancelOrder request raced this saga) - whatever Step 1 already did was compensated
            // by the orchestrator (inventory restocked), and retrying would just throw the same
            // exception forever, so this is a safe no-op rather than rethrown for Inbox to retry.
            logger.Warning(
                "CreateOrderSaga for order {OrderId} stopped: {Message}",
                request.OrderId, statusEx.Message);
        }

        // Any other SagaExecutionException (e.g. a transient DB error inside ConfirmOrderStep) is
        // intentionally left unhandled here: DeductInventoryStep's compensation has already
        // restocked, so the order is back to a clean Pending state and rethrowing lets Inbox's
        // retry/backoff redeliver OrderCreatedIntegrationEvent - DeductStock/RestockStock are
        // idempotent per OrderId, so re-running the whole saga is always safe.
    }

    private async Task CancelOrderAsync(Guid orderId, Guid customerId, string reason, CancellationToken ct)
    {
        var (tenantId, _) = await orderWriteService.CancelAsync(orderId, reason, ct);

        await outboxStore.EnqueueAsync(new OrderCancelledIntegrationEvent(tenantId, orderId, customerId, reason), ct);

        await uow.SaveChangesAsync(ct);

        logger.Information("Order {OrderId} cancelled due to inventory failure ({Reason})", orderId, reason);
    }
}
