namespace NovaCore.Order.Application.Features.Orders.Queries.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IQuery<GetOrderResponse>;

public sealed record GetOrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Discount,
    decimal LineTotal);

public sealed record GetOrderResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    string ShippingAddress,
    OrderStatus Status,
    decimal TotalAmount,
    IReadOnlyCollection<GetOrderItemResponse> Items,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);
