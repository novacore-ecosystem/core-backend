using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.Interactions.Queries.GetMyInteractions;

namespace NovaCore.Chat.API.Endpoints.Interactions;

/// <summary>Cursor-paginated feed of mentions/reactions involving the caller - see GetMyInteractionsHandler.</summary>
public sealed class GetMyInteractionsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get My Interactions",
        "",
        "Cursor-paginated feed of items where the caller was mentioned, or where someone reacted",
        "to a message the caller sent - most recent first.",
        "",
        "### Query Parameters",
        "- **cursor** (optional): opaque cursor from a previous page's response",
        "- **limit** (optional, default 20)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/interactions/mine", Handle)
            .WithTags("Interactions")
            .RequireAuthorization()
            .WithName("GetMyInteractions")
            .WithDisplayName("Get My Interactions API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CursorPaginatedResult<MyInteractionDto>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery(Name = "cursor")] string? cursor,
        [FromServices] ISender sender,
        [FromQuery(Name = "limit")] int limit = 20,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetMyInteractionsQuery(cursor, limit), ct);

        return Results.Ok(ApiResponse<CursorPaginatedResult<MyInteractionDto>>.Ok(response));
    }
}
