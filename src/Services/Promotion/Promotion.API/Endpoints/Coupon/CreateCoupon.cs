using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Coupons.Commands.CreateCoupon;

namespace NovaCore.Promotion.API.Endpoints.Coupon;

public sealed record CreateCouponRequest(
    Guid PromotionId,
    string Code,
    string Name,
    CouponType CouponType,
    DateTime StartTime,
    DateTime EndTime,
    string TimeZone,
    string? Description,
    CouponVisibility Visibility,
    Guid? CampaignId,
    Guid? BatchId,
    int? MaxUsage,
    int? MaxUsagePerUser);

public sealed class CreateCouponEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Coupon",
        "",
        "Creates a Coupon under an existing Promotion.",
        "",
        "### Request Body",
        "- **PromotionId**: The Promotion this Coupon belongs to (required)",
        "- **Code**: Coupon code (required)",
        "- **Name**: Coupon name (required)",
        "- **CouponType/StartTime/EndTime/TimeZone**: Required",
        "- **Description/Visibility/CampaignId/BatchId/MaxUsage/MaxUsagePerUser**: Optional",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/coupons", Handle)
            .WithTags("Coupon")
            .RequireAuthorization()
            .WithName("CreateCoupon")
            .WithDisplayName("Create Coupon API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateCouponResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateCouponRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateCouponCommand(
            request.PromotionId,
            request.Code.Trim(),
            request.Name.Trim(),
            request.CouponType,
            request.StartTime,
            request.EndTime,
            request.TimeZone,
            request.Description?.Trim(),
            request.Visibility,
            request.CampaignId,
            request.BatchId,
            request.MaxUsage,
            request.MaxUsagePerUser);

        var response = await sender.Send(command, ct);

        return Results.Created($"/coupons/{response.CouponId}", ApiResponse<CreateCouponResponse>.Ok(response));
    }
}
