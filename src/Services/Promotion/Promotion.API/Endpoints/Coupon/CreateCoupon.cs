using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Coupons.Commands.CreateCoupon;

namespace NovaCore.Promotion.API.Endpoints.Coupon;

public sealed record CreateCouponRequest(string Name);

/// <summary>
/// Phase 4.1 CQRS/Minimal API skeleton - the representative Coupon write flow future features
/// clone. Deliberately minimal (Name only), not a real Coupon-creation contract - see
/// CreateCouponHandler.
/// </summary>
public sealed class CreateCouponEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Coupon (skeleton)",
        "",
        "Phase 4.1 CQRS/Minimal API skeleton only - not a real Coupon-creation feature yet.",
        "",
        "### Request Body",
        "- **Name**: Coupon name (required)",
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
        var command = new CreateCouponCommand(request.Name.Trim());

        var response = await sender.Send(command, ct);

        return Results.Created($"/coupons/{response.CouponId}", ApiResponse<CreateCouponResponse>.Ok(response));
    }
}
