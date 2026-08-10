namespace NovaCore.Order.Application.Features.Orders.DTOs;

public sealed record OrderItemRequestDto(
    Guid ProductId,
    Guid VariationId,
    int Quantity);
