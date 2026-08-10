namespace NovaCore.Order.Application.Features.Orders.Commands.UpdateOrderOwnerInfo;

public sealed record UpdateOrderOwnerInfoCommand(
    Guid OrderId,
    string OwnerName,
    Email OwnerEmail,
    PhoneNumber OwnerPhone) : ICommand;
