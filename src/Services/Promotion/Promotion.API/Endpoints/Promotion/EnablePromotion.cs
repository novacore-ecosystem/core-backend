using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Commands.EnablePromotion;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed class EnablePromotionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Enable Promotion",
        "",
        "Enables a Promotion (IsEnabled = true). Independent of lifecycle Status.",
        "",
        "### Route Parameters",
        "- **promotionId**: Unique identifier of the promotion (required)",
        "",
        "### Error Responses",
        "- **404**: Promotion not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/{promotionId}/enable", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("EnablePromotion")
            .WithDisplayName("Enable Promotion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<EnablePromotionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid promotionId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new EnablePromotionCommand(promotionId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<EnablePromotionResponse>.Ok(response));
    }
}
