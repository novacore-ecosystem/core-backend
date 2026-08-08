using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Coupons.Queries.ListCoupons;

namespace NovaCore.Promotion.API.Endpoints.Coupon;

public sealed class ListCouponsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## List Coupons",
        "",
        "Paginated, filterable administrative list of Coupons.",
        "",
        "### Query Parameters",
        "- **status**: Filter by Coupon status (optional)",
        "- **page**: Page number, 1-based (default 1)",
        "- **pageSize**: Items per page (default 20)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/coupons", Handle)
            .WithTags("Coupon")
            .RequireAuthorization()
            .WithName("ListCoupons")
            .WithDisplayName("List Coupons API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<CouponSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery] CouponStatus? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListCouponsQuery(
            status,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<CouponSummaryResponse>>.Ok(response));
    }
}
