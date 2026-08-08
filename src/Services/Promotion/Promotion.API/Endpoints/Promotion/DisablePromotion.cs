using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Commands.DisablePromotion;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed class DisablePromotionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Disable Promotion",
        "",
        "Disables a Promotion (IsEnabled = false). Independent of lifecycle Status - a disabled",
        "Promotion is excluded from evaluation regardless of Status.",
        "",
        "### Route Parameters",
        "- **promotionId**: Unique identifier of the promotion (required)",
        "",
        "### Error Responses",
        "- **404**: Promotion not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/{promotionId}/disable", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("DisablePromotion")
            .WithDisplayName("Disable Promotion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<DisablePromotionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid promotionId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new DisablePromotionCommand(promotionId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<DisablePromotionResponse>.Ok(response));
    }
}
