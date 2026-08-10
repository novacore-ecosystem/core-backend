using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Cart.Commands.AddCartItem;

public sealed record AddCartItemCommand(Guid VariationId, int Quantity) : ICommand<CartResponse>;
