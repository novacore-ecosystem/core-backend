using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Queries.EvaluatePromotions;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed record EvaluatePromotionsRequest(
    Guid? UserId,
    string Currency,
    decimal OrderAmount,
    IReadOnlyList<EvaluationItemRequest> Items);

/// <summary>
/// PromotionService calculates Promotion effects only - never a final Order total (OrderService's
/// responsibility). No OrderService/ProductService/InventoryService dependency - the caller sends
/// the snapshot this evaluation needs.
/// </summary>
public sealed class EvaluatePromotionsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Evaluate Promotions",
        "",
        "Evaluates active, enabled, automatically-triggered Promotions against the given order",
        "context and returns the discount(s) that apply - never a final Order total. Deterministic:",
        "same configuration + context + evaluation time always yields the same result.",
        "",
        "### Request Body",
        "- **userId**: Current user/account, for Customer-targeted Promotions (optional)",
        "- **currency**: ISO-4217 currency code (required)",
        "- **orderAmount**: Order base amount before any discount (required)",
        "- **items**: Order line items - productId/variantId/categoryId/quantity/unitPrice (required)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/promotions/evaluate", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("EvaluatePromotions")
            .WithDisplayName("Evaluate Promotions API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<EvaluatePromotionsResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromBody] EvaluatePromotionsRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new EvaluatePromotionsQuery(
            request.UserId,
            request.Currency,
            request.OrderAmount,
            request.Items);

        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<EvaluatePromotionsResponse>.Ok(response));
    }
}
