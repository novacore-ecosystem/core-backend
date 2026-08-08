using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Commands.AddPromotionExclusion;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed class AddPromotionExclusionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Add Promotion Exclusion",
        "",
        "Records that this Promotion cannot stack with another Promotion, regardless of either",
        "Promotion's StackingMode - checked by Evaluate Promotion's stacking pass.",
        "",
        "### Route Parameters",
        "- **promotionId**: The Promotion the exclusion applies to (required)",
        "- **excludedPromotionId**: The Promotion it cannot stack with (required)",
        "",
        "### Error Responses",
        "- **404**: Either Promotion not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/{promotionId}/exclusions/{excludedPromotionId}", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("AddPromotionExclusion")
            .WithDisplayName("Add Promotion Exclusion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<AddPromotionExclusionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid promotionId,
        [FromRoute] Guid excludedPromotionId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new AddPromotionExclusionCommand(promotionId, excludedPromotionId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<AddPromotionExclusionResponse>.Ok(response));
    }
}
