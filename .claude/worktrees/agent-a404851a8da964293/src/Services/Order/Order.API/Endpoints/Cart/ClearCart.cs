using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Order.Application.Features.Cart.Commands.ClearCart;

namespace NovaCore.Order.API.Endpoints.Cart;

public sealed class ClearCartEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Clear Cart",
        "",
        "Empties the current user's cart entirely.",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/cart", Handle)
            .WithTags("Cart")
            .RequireAuthorization()
            .WithName("ClearCart")
            .WithDisplayName("Clear Cart API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<object>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        await sender.Send(new ClearCartCommand(), ct);

        return Results.Ok(ApiResponse<object>.Ok());
    }
}
