using NovaCore.Order.Application.Features.Orders.DTOs;

namespace NovaCore.Order.Application.Features.Orders.Commands.CreateOrder;

/// <summary>CustomerId is deliberately absent - the client path always derives identity from the caller's JWT claim (see CreateOrderHandler). AdminCreateOrderCommand is the on-behalf-of counterpart.</summary>
public sealed record CreateOrderCommand(
    OrderOwnerRequestDto Owner,
    OrderShippingInfoRequestDto ShippingInfo,
    OrderItemRequestDto[] Items,
    Guid? CreatedById = null) : ICommand<CreateOrderResponse>;

public sealed record CreateOrderResponse(
    Guid OrderId,
    string OrderNumber);
