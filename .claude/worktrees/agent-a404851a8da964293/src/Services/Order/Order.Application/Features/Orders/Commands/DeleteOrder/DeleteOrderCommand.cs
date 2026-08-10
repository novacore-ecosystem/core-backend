namespace NovaCore.Order.Application.Features.Orders.Commands.DeleteOrder;

public sealed record DeleteOrderCommand(Guid OrderId) : ICommand<DeleteOrderResponse>;

public sealed record DeleteOrderResponse;
