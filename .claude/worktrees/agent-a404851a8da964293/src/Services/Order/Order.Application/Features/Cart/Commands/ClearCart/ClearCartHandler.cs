using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Exceptions;

using NovaCore.Order.Application.Abstractions.Services;

namespace NovaCore.Order.Application.Features.Cart.Commands.ClearCart;

public sealed class ClearCartHandler(
    ICurrentUserService currentUser,
    ICartService cartService) : ICommandHandler<ClearCartCommand>
{
    public async Task Handle(ClearCartCommand request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId() ?? throw new ForbiddenException();

        await cartService.ClearCartAsync(userId, ct);
    }
}
