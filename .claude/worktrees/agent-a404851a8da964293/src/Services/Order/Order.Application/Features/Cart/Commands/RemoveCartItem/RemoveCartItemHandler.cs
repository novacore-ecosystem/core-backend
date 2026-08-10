using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Cart.Commands.RemoveCartItem;

public sealed class RemoveCartItemHandler(
    ICurrentUserService currentUser,
    ICartService cartService) : ICommandHandler<RemoveCartItemCommand, CartResponse>
{
    public async Task<CartResponse> Handle(RemoveCartItemCommand request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId() ?? throw new ForbiddenException();

        return await cartService.RemoveItemAsync(userId, request.VariationId, ct);
    }
}
