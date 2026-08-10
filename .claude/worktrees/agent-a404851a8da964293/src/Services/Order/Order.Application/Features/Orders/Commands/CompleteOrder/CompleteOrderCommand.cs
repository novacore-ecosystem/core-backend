namespace NovaCore.Order.Application.Features.Orders.Commands.CompleteOrder;

public sealed record CompleteOrderCommand(Guid OrderId) : ICommand<CompleteOrderResponse>;

public sealed record CompleteOrderResponse(Guid OrderId, OrderStatus Status);
