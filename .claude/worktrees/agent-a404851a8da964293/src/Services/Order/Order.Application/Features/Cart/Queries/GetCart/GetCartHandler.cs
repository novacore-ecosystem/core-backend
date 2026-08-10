using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Cart.Queries.GetCart;

public sealed class GetCartHandler(
    ICurrentUserService currentUser,
    ICartService cartService) : IQueryHandler<GetCartQuery, CartResponse>
{
    public async Task<CartResponse> Handle(GetCartQuery request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId()
            ?? throw new ForbiddenException();

        var (_, cart) = await cartService.GetCartAsync(userId, ct);
        return cart;
    }
}
