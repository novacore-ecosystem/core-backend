using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Coupons.Commands.UpdateCoupon;

namespace NovaCore.Promotion.API.Endpoints.Coupon;

public sealed record UpdateCouponRequest(
    string Name,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    string TimeZone,
    CouponVisibility Visibility,
    int? MaxUsage,
    int? MaxUsagePerUser);

public sealed class UpdateCouponEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Coupon",
        "",
        "Updates a Coupon's administrative details, schedule, visibility, and usage limits.",
        "Does not change lifecycle status - see the dedicated status-transition operations.",
        "",
        "### Route Parameters",
        "- **couponId**: Unique identifier of the coupon (required)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **404**: Coupon not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/coupons/{couponId}", Handle)
            .WithTags("Coupon")
            .RequireAuthorization()
            .WithName("UpdateCoupon")
            .WithDisplayName("Update Coupon API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdateCouponResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid couponId,
        [FromBody] UpdateCouponRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new UpdateCouponCommand(
            couponId,
            request.Name.Trim(),
            request.Description?.Trim(),
            request.StartTime,
            request.EndTime,
            request.TimeZone,
            request.Visibility,
            request.MaxUsage,
            request.MaxUsagePerUser);

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<UpdateCouponResponse>.Ok(response));
    }
}
