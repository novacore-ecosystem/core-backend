namespace NovaCore.Order.Application.Features.Orders.DTOs;

public sealed record OrderShippingInfoRequestDto(
    ShippingMethod ShippingMethod,
    string ReceiverName,
    PhoneNumber ReceiverPhone,
    string ShippingAddress,
    string Note);
