using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Cart.Commands.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityCommand(Guid VariationId, int Quantity) : ICommand<CartResponse>;
