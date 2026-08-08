using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Commands.ApprovePromotion;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed class ApprovePromotionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Approve Promotion",
        "",
        "Approves the Promotion's pending ApprovalWorkflow and activates the Promotion - there is",
        "no separate Activate operation, since PromotionStatus has no intermediate Scheduled state",
        "and Promotion.Activate() itself requires approval.",
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
        app.MapPost("/promotions/{promotionId}/approve", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("ApprovePromotion")
            .WithDisplayName("Approve Promotion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<ApprovePromotionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid promotionId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new ApprovePromotionCommand(promotionId);
        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<ApprovePromotionResponse>.Ok(response));
    }
}
