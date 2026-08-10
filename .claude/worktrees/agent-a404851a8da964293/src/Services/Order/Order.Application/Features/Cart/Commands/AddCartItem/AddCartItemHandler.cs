using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Cart.Commands.AddCartItem;

public sealed class AddCartItemHandler(
    ICurrentUserService currentUser,
    ICartService cartService) : ICommandHandler<AddCartItemCommand, CartResponse>
{
    public async Task<CartResponse> Handle(AddCartItemCommand request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId() ?? throw new ForbiddenException();

        return await cartService.AddItemAsync(userId, request.VariationId, request.Quantity, ct);
    }
}
