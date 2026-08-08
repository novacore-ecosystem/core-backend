using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Coupons.Commands.DisableCoupon;

namespace NovaCore.Promotion.API.Endpoints.Coupon;

public sealed class DisableCouponEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Disable Coupon",
        "",
        "Disables a Coupon (IsEnabled = false). Historical Coupon data is never physically deleted.",
        "",
        "### Route Parameters",
        "- **couponId**: Unique identifier of the coupon (required)",
        "",
        "### Error Responses",
        "- **404**: Coupon not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/coupons/{couponId}/disable", Handle)
            .WithTags("Coupon")
            .RequireAuthorization()
            .WithName("DisableCoupon")
            .WithDisplayName("Disable Coupon API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<DisableCouponResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid couponId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new DisableCouponCommand(couponId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<DisableCouponResponse>.Ok(response));
    }
}
