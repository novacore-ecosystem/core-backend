using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Coupons.Commands.TranslateCoupon;

namespace NovaCore.Promotion.API.Endpoints.Coupon;

public sealed record TranslateCouponRequest(string Name, string? Description);

/// <summary>
/// One upsert endpoint - no separate Create/Update translation routes (Coupon.Translate is
/// already an upsert; see TranslateCouponCommand).
/// </summary>
public sealed class TranslateCouponEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Translate Coupon",
        "",
        "Upserts a Coupon's translation for the given language - creates it if missing, updates it otherwise.",
        "",
        "### Route Parameters",
        "- **couponId**: Unique identifier of the coupon (required)",
        "- **languageCode**: Target language code (required)",
        "",
        "### Request Body",
        "- **Name**: Translated coupon name (required)",
        "- **Description**: Translated coupon description (optional)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **404**: Coupon not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/coupons/{couponId}/translations/{languageCode}", Handle)
            .WithTags("Coupon")
            .RequireAuthorization()
            .WithName("TranslateCoupon")
            .WithDisplayName("Translate Coupon API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<TranslateCouponResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid couponId,
        [FromRoute] string languageCode,
        [FromBody] TranslateCouponRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new TranslateCouponCommand(couponId, languageCode, request.Name.Trim(), request.Description?.Trim());
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<TranslateCouponResponse>.Ok(response));
    }
}
