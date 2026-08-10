namespace NovaCore.Order.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, string Reason = "CancelledByCustomer") : ICommand<CancelOrderResponse>;

public sealed record CancelOrderResponse(Guid OrderId, OrderStatus Status);
