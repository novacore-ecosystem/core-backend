namespace NovaCore.Order.Application.Features.Orders.DTOs;

public sealed record OrderOwnerRequestDto(
    Guid OwnerId,
    string OwnerName,
    Email OwnerEmail,
    PhoneNumber OwnerPhone);