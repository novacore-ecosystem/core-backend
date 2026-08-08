using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Commands.CreatePromotion;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed record CreatePromotionRequest(
    string Code,
    string Name,
    PromotionType Type,
    DateTime StartTime,
    DateTime EndTime,
    string Currency,
    string TimeZone,
    string? Description,
    int Priority,
    Guid? CampaignId,
    IReadOnlyList<CreatePromotionBenefitRequest>? Benefits,
    IReadOnlyList<CreatePromotionTargetRequest>? Targets,
    IReadOnlyList<CreatePromotionConstraintRequest>? Constraints,
    PromotionStackingMode StackingMode);

public sealed class CreatePromotionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Promotion",
        "",
        "Creates a Promotion, with its Benefits/Targets/Constraints/StackingMode set once at",
        "creation (this phase has no separate endpoint to change them afterward).",
        "",
        "### Request Body",
        "- **Code/Name/Type/StartTime/EndTime/Currency/TimeZone**: Required",
        "- **Description/Priority/CampaignId**: Optional",
        "- **Benefits/Targets/Constraints**: Optional initial configuration",
        "- **StackingMode**: Optional, defaults to NotStackable",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("CreatePromotion")
            .WithDisplayName("Create Promotion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreatePromotionResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreatePromotionRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreatePromotionCommand(
            request.Code.Trim(),
            request.Name.Trim(),
            request.Type,
            request.StartTime,
            request.EndTime,
            request.Currency,
            request.TimeZone,
            request.Description?.Trim(),
            request.Priority,
            request.CampaignId,
            request.Benefits,
            request.Targets,
            request.Constraints,
            request.StackingMode);

        var response = await sender.Send(command, ct);

        return Results.Created($"/promotions/{response.PromotionId}", ApiResponse<CreatePromotionResponse>.Ok(response));
    }
}
