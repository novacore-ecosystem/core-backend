using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Commands.SubmitPromotion;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed class SubmitPromotionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Submit Promotion",
        "",
        "Submits a Draft Promotion for approval - creates and starts an ApprovalWorkflow.",
        "",
        "### Route Parameters",
        "- **promotionId**: Unique identifier of the promotion (required)",
        "",
        "### Error Responses",
        "- **400**: Promotion is not in Draft status, or already has an approval workflow in progress",
        "- **404**: Promotion not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/{promotionId}/submit", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("SubmitPromotion")
            .WithDisplayName("Submit Promotion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<SubmitPromotionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid promotionId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new SubmitPromotionCommand(promotionId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<SubmitPromotionResponse>.Ok(response));
    }
}
