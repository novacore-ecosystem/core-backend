using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Commands.RejectPromotion;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed class RejectPromotionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Reject Promotion",
        "",
        "Rejects the Promotion's pending ApprovalWorkflow. The Promotion stays Draft and its",
        "approval workflow link is cleared, so it can be resubmitted with a new workflow.",
        "",
        "### Route Parameters",
        "- **promotionId**: Unique identifier of the promotion (required)",
        "",
        "### Error Responses",
        "- **400**: Promotion has not been submitted for approval, or its workflow is not Pending",
        "- **404**: Promotion not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/{promotionId}/reject", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("RejectPromotion")
            .WithDisplayName("Reject Promotion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<RejectPromotionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid promotionId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new RejectPromotionCommand(promotionId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<RejectPromotionResponse>.Ok(response));
    }
}
