using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Commands.UpdatePromotion;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed record UpdatePromotionRequest(
    string Name,
    string? Description,
    DateTime StartTime,
    DateTime EndTime,
    string TimeZone,
    int Priority);

public sealed class UpdatePromotionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Update Promotion",
        "",
        "Updates a Promotion's name, description, schedule, and priority. Does not change",
        "lifecycle/approval status or Benefits/Targets/Constraints - see the dedicated operations.",
        "",
        "### Route Parameters",
        "- **promotionId**: Unique identifier of the promotion (required)",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **404**: Promotion not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/promotions/{promotionId}", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("UpdatePromotion")
            .WithDisplayName("Update Promotion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<UpdatePromotionResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid promotionId,
        [FromBody] UpdatePromotionRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new UpdatePromotionCommand(
            promotionId,
            request.Name.Trim(),
            request.Description?.Trim(),
            request.StartTime,
            request.EndTime,
            request.TimeZone,
            request.Priority);

        var response = await sender.Send(command, ct);

        return Results.Ok(ApiResponse<UpdatePromotionResponse>.Ok(response));
    }
}
