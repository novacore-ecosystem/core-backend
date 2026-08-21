using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.ConversationQueues.Queries.GetConversationQueue;

namespace NovaCore.Chat.API.Endpoints.ConversationQueues;

/// <summary>CS/admin discovery feed - cursor-paginated over ConversationQueueItem, see GetConversationQueueHandler.</summary>
public sealed class GetConversationQueueEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Conversation Queue",
        "",
        "Cursor-paginated list of conversations currently waiting for CS assignment, ordered by",
        "enqueue time.",
        "",
        "### Query Parameters",
        "- **queueId** (optional): restrict to one ConversationQueue",
        "- **cursor** (optional): opaque cursor from a previous page's response",
        "- **limit** (optional, default 20)",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/conversation-queues/items", Handle)
            .WithTags("ConversationQueues")
            .RequireAuthorization()
            .WithName("GetConversationQueue")
            .WithDisplayName("Get Conversation Queue API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CursorPaginatedResult<ConversationQueueItemDto>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery(Name = "queueId")] Guid? queueId,
        [FromQuery(Name = "cursor")] string? cursor,
        [FromServices] ISender sender,
        [FromQuery(Name = "limit")] int limit = 20,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetConversationQueueQuery(queueId, cursor, limit), ct);

        return Results.Ok(ApiResponse<CursorPaginatedResult<ConversationQueueItemDto>>.Ok(response));
    }
}
