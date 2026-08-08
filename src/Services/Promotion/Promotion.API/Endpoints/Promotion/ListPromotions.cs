using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Promotion.Application.Features.Promotions.Queries.ListPromotions;

namespace NovaCore.Promotion.API.Endpoints.Promotion;

public sealed class ListPromotionsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## List Promotions",
        "",
        "Paginated, filterable administrative list of Promotions.",
        "",
        "### Query Parameters",
        "- **status**: Filter by Promotion status (optional)",
        "- **page**: Page number, 1-based (default 1)",
        "- **pageSize**: Items per page (default 20)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/promotions", Handle)
            .WithTags("Promotion")
            .RequireAuthorization()
            .WithName("ListPromotions")
            .WithDisplayName("List Promotions API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<PaginatedResult<PromotionSummaryResponse>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery] PromotionStatus? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new ListPromotionsQuery(
            status,
            page is null or <= 0 ? 1 : page.Value,
            pageSize is null or <= 0 ? 20 : pageSize.Value);

        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<PaginatedResult<PromotionSummaryResponse>>.Ok(response));
    }
}
