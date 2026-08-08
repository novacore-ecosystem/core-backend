using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Infrastructure.Idempotency;
using NovaCore.BuildingBlock.SharedKernel.Constants;
using NovaCore.BuildingBlock.SharedKernel.Extensions;
using NovaCore.BuildingBlock.Web.Swagger.EndpointHeader;

using NovaCore.Promotion.Application.Features.Coupons.Commands.RedeemCoupon;

namespace NovaCore.Promotion.API.Endpoints.Coupon;

public sealed record RedeemCouponRequest(string Code, Guid? OrderId);

/// <summary>
/// Establishes the Coupon's usage/claim state for the current user - never calculates a discount
/// (Promotion Engine logic, a later phase). Redemption re-checks eligibility live rather than
/// trusting a prior ValidateCoupon call (see RedeemCouponHandler).
/// </summary>
public sealed class RedeemCouponEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Redeem Coupon",
        "",
        "Records a Coupon redemption for the current user - increments usage and creates a",
        "CouponUsage record. Transactional, concurrency-safe, and idempotent: retrying with the",
        "same Idempotency-Key header replays the original result, and retrying with the same",
        "Coupon+User+Order combination returns the existing redemption instead of creating a second",
        "one. Never calculates a discount or order total.",
        "",
        "### Request Body",
        "- **code**: The Coupon code to redeem (required)",
        "- **orderId**: External Order reference this redemption is for (optional)",
        "",
        "### Error Responses",
        "- **404**: Coupon not found",
        "- **400**: Coupon is disabled, not currently active, or its usage limit has been reached",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/coupons/redeem", Handle)
            .WithTags("Coupon")
            .RequireAuthorization()
            .Headers([
                new HeaderDefinition(HeaderKeyConstant.IdempotencyKey, true, "Ensures this Coupon is only redeemed once, even if the request is retried")
            ])
            .RequireIdempotency()
            .WithName("RedeemCoupon")
            .WithDisplayName("Redeem Coupon API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<RedeemCouponResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] RedeemCouponRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new RedeemCouponCommand(request.Code, request.OrderId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<RedeemCouponResponse>.Ok(response));
    }
}
