using NovaCore.BuildingBlock.Contract.Events.Order;

namespace NovaCore.Order.Application.Features.Orders.Commands.RunCreateOrderSaga;

/// <summary>Dispatched by OrderCreatedSagaConsumer (a thin adapter - see docs/reference/create-order-saga.md) once per OrderCreatedIntegrationEvent delivery.</summary>
public sealed record RunCreateOrderSagaCommand(
    Guid OrderId,
    Guid CustomerId,
    IReadOnlyCollection<OrderCreatedItem> Items,
    string? CorrelationId) : ICommand;
