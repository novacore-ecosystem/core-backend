namespace NovaCore.Order.Application.Features.Orders.Sagas.CreateOrderSaga;

/// <summary>
/// Thrown by DeductInventoryStep when Inventory reports a business failure (insufficient stock,
/// unknown variation) rather than a transport/system error. Caught by OrderCreatedSagaConsumer to
/// distinguish "cancel the order, OutOfStock, saga ends" from "unexpected failure, let Inbox retry
/// the whole saga" - see docs/reference/create-order-saga.md.
/// </summary>
public sealed class InventoryDeductionFailedException(string failureCode)
    : Exception($"Inventory deduction failed: {failureCode}")
{
    public string FailureCode { get; } = failureCode;
}
