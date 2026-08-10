namespace NovaCore.Order.Application.Features.Orders.Sagas.CreateOrderSaga;

/// <summary>Keys into ISagaContext's data bag - see SagaContext.Get/Set. Internal: only CreateOrderSagaConsumer builds the context, only this saga's own steps read it.</summary>
internal static class CreateOrderSagaContextKeys
{
    public const string OrderId = "OrderId";
    public const string CustomerId = "CustomerId";
    public const string Items = "Items";
    public const string FailureReason = "FailureReason";
}
