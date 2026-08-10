using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Order.Application.Features.Orders.Queries.GetOrder;

namespace NovaCore.Order.API.Endpoints.Order;

public sealed class GetOrderEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Order Details",
        "",
        "Retrieves order information by order ID.",
        "",
        "### Route Parameters",
        "- **orderId**: Unique identifier of the order (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Order not found",
        "- **400**: Invalid orderId format",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderId}", Handle)
            .WithTags("Order")
            .RequireAuthorization()
            .WithName("GetOrder")
            .WithDisplayName("Get Order API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetOrderResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetOrderQuery(orderId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetOrderResponse>.Ok(response));
    }
}
