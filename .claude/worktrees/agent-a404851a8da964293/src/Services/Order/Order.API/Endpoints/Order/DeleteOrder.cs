using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Web.Authorization;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Order.Application.Features.Orders.Commands.DeleteOrder;

namespace NovaCore.Order.API.Endpoints.Order;

public sealed class DeleteOrderEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Delete Order",
        "",
        "Hard-deletes an order. Only Pending or Cancelled orders can be deleted - both states",
        "never had stock deducted, so no Inventory compensation is triggered. Confirmed/Completed",
        "orders must be cancelled through the normal fulfillment flow instead, never deleted.",
        "",
        "### Route Parameters",
        "- **orderId**: Unique identifier of the order (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Order not found",
        "- **400**: Order is Confirmed or Completed and cannot be deleted",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/orders/{orderId}", Handle)
            .WithTags("Order")
            .RequirePermissions(Permissions.Order.Delete)
            .WithName("DeleteOrder")
            .WithDisplayName("Delete Order API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<DeleteOrderResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid orderId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new DeleteOrderCommand(orderId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<DeleteOrderResponse>.Ok(response));
    }
}
