using NovaCore.BuildingBlock.Saga.Abstractions;
using NovaCore.BuildingBlock.Saga.Core;

using NovaCore.Order.Application.Features.Orders.Sagas.CreateOrderSaga.Steps;

namespace NovaCore.Order.Application.Features.Orders.Sagas.CreateOrderSaga;

/// <summary>
/// Builds the CreateOrderSaga workflow: DeductInventory -> ConfirmOrder. Kept as its own factory
/// (rather than inlined in the consumer) so future workflows - Payment, Shipping, Admin Approval,
/// etc. - can each get their own SagaDefinitionBuilder without touching this one, per the
/// "future compatibility" goal in docs/reference/create-order-saga.md.
/// </summary>
public static class CreateOrderSagaDefinitionFactory
{
    public const string SagaName = "CreateOrderSaga";

    public static ISagaDefinition Create(DeductInventoryStep deductInventoryStep, ConfirmOrderStep confirmOrderStep) =>
        new SagaDefinitionBuilder(SagaName)
            .Description(
                "Deducts inventory for a newly created Pending order, then confirms it. " +
                "On inventory failure the order is cancelled (OutOfStock) instead of completing the saga.")
            .AddSteps(deductInventoryStep, confirmOrderStep)
            .Build();
}
