using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Cart.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid VariationId) : ICommand<CartResponse>;
