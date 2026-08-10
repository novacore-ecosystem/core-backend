using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Order.Application.Abstractions.Services;
using NovaCore.Order.Application.Features.Cart.Commands.AddCartItem;

namespace NovaCore.Order.API.Endpoints.Cart;

public sealed record AddCartItemRequest(Guid VariationId, int Quantity);

public sealed class AddCartItemEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Add Cart Item",
        "",
        "Adds a product variation to the current user's cart. If the variation is already in the",
        "cart, Quantity is added to the existing line instead of creating a duplicate.",
        "",
        "### Request Body",
        "- **VariationId**: Id of the product variation to add (required, must exist and be Active)",
        "- **Quantity**: Quantity to add (required, greater than 0)",
        "",
        "### Error Responses",
        "- **404**: Variation not found in the local product catalog",
        "- **400**: Product is not currently available for ordering, or requested quantity",
        "  (existing cart quantity + this request) exceeds real-time Inventory stock - the 400's",
        "  `details` carries `{ insufficients: [variationId] }`",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/cart/items", Handle)
            .WithTags("Cart")
            .RequireAuthorization()
            .WithName("AddCartItem")
            .WithDisplayName("Add Cart Item API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CartResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] AddCartItemRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new AddCartItemCommand(request.VariationId, request.Quantity);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<CartResponse>.Ok(response));
    }
}
