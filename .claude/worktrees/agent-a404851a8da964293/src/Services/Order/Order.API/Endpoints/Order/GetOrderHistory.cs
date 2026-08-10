using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Criteria.Requests;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Order.Application.Features.Orders.Queries.GetOrderHistory;

namespace NovaCore.Order.API.Endpoints.Order;

public sealed class GetOrderHistoryEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Order History",
        "",
        "Lists the calling user's own orders - the customer-facing counterpart to POST",
        "/orders/search (admin-only, any customer). The customer identity always comes from the",
        "caller's own auth token; there is no way to request another customer's orders through",
        "this endpoint.",
        "",
        "### Request Body",
        "- **Filters**: List of `{ field, operator, value }` - allowed fields: status, createdAt",
        "- **Sorts**: List of `{ field, direction }` - sortable fields: status, createdAt",
        "- **Page** / **PageSize**: Pagination (PageSize 1-200, default 20)",
        "",
        "### Error Responses",
        "- **400**: Unknown field, disallowed operator for a field, or malformed filter/sort value",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders/history", Handle)
            .WithTags("Order")
            .RequireAuthorization()
            .WithName("GetOrderHistory")
            .WithDisplayName("Order History API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<OrderHistoryItemResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] CriteriaRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetOrderHistoryQuery(request);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<OrderHistoryItemResponse>>.Ok(response));
    }
}
