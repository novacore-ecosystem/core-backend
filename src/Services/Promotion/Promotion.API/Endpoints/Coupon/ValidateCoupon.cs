using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Coupons.Queries.ValidateCoupon;

namespace NovaCore.Promotion.API.Endpoints.Coupon;

/// <summary>Answers "is this Coupon currently valid?" only - never Order/product/customer eligibility. See RedeemCoupon for the state-changing counterpart.</summary>
public sealed class ValidateCouponEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Validate Coupon",
        "",
        "Checks whether a Coupon code is currently valid for redemption by the current user - status,",
        "enabled flag, active time window, and usage limits. Does not calculate a discount, does not",
        "reserve or redeem the Coupon, and does not evaluate Order/product/customer-segment",
        "eligibility (Promotion Engine logic, a later phase).",
        "",
        "### Query Parameters",
        "- **code**: The Coupon code to validate (required)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/coupons/validate", Handle)
            .WithTags("Coupon")
            .RequireAuthorization()
            .WithName("ValidateCoupon")
            .WithDisplayName("Validate Coupon API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<ValidateCouponResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery] string code,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ValidateCouponQuery(code);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<ValidateCouponResponse>.Ok(response));
    }
}
