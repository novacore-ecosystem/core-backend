using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Commands.CancelPromotion;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed class CancelPromotionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Delete Promotion",
        "",
        "Cancels a Promotion (Promotion.Cancel()). Historical Promotion data is never physically",
        "deleted.",
        "",
        "### Route Parameters",
        "- **promotionId**: Unique identifier of the promotion (required)",
        "",
        "### Error Responses",
        "- **404**: Promotion not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/promotions/{promotionId}", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("CancelPromotion")
            .WithDisplayName("Delete Promotion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CancelPromotionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid promotionId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CancelPromotionCommand(promotionId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<CancelPromotionResponse>.Ok(response));
    }
}
