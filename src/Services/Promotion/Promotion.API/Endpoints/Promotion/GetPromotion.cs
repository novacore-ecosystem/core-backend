using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Queries.GetPromotion;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed class GetPromotionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Promotion",
        "",
        "Administrative Promotion detail - status, lifecycle/approval state, and configured",
        "Benefits/Targets/Constraints/StackingMode.",
        "",
        "### Route Parameters",
        "- **promotionId**: Unique identifier of the promotion (required)",
        "",
        "### Error Responses",
        "- **404**: Promotion not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions/{promotionId}", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("GetPromotion")
            .WithDisplayName("Get Promotion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetPromotionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid promotionId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetPromotionQuery(promotionId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetPromotionResponse>.Ok(response));
    }
}
