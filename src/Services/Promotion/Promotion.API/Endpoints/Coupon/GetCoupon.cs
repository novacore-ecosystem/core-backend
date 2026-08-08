using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Coupons.Queries.GetCoupon;

namespace NovaCore.Promotion.API.Endpoints.Coupon;

/// <summary>
/// Phase 4.1 CQRS/Minimal API skeleton - the representative Coupon read flow future features
/// clone. See GetCouponHandler.
/// </summary>
public sealed class GetCouponEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Coupon (skeleton)",
        "",
        "Phase 4.1 CQRS/Minimal API skeleton only - not a real Coupon-lookup feature yet.",
        "",
        "### Route Parameters",
        "- **couponId**: Unique identifier of the coupon (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Coupon not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/coupons/{couponId}", Handle)
            .WithTags("Coupon")
            .RequireAuthorization()
            .WithName("GetCoupon")
            .WithDisplayName("Get Coupon API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetCouponResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid couponId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetCouponQuery(couponId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetCouponResponse>.Ok(response));
    }
}
