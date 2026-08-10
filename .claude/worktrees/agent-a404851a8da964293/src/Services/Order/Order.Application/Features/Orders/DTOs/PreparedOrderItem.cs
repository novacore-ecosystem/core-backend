namespace NovaCore.Order.Application.Features.Orders.DTOs;

public sealed record PreparedOrderItem(
    Guid ProductId,
    Guid VariationId,
    string ProductName,
    string VariationName,
    decimal UnitPrice,
    int Quantity);
