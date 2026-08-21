using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.Conversations.Queries.GetConversationStatus;

namespace NovaCore.Chat.API.Endpoints.Conversations;

/// <summary>
/// Recovery-check endpoint - a client that persisted a conversation id locally calls this to decide
/// whether to resume it or start a new one. Falls back for a client that missed the ChatHub
/// ConversationClosed broadcast (see IChatHubClient.ConversationClosed).
/// </summary>
public sealed class GetConversationStatusEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Conversation Status",
        "",
        "Minimal status payload for deciding whether a locally-persisted conversation can still",
        "be resumed. Requires the caller (guest or user) to be a participant/contact of the",
        "conversation.",
        "",
        "### Error Responses",
        "- **403**: Caller is not a participant/contact of this conversation",
        "- **404**: Conversation not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/conversations/{conversationId}/status", Handle)
            .WithTags("Conversations")
            .RequireAuthorization()
            .WithName("GetConversationStatus")
            .WithDisplayName("Get Conversation Status API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetConversationStatusResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid conversationId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetConversationStatusQuery(conversationId), ct);

        return Results.Ok(ApiResponse<GetConversationStatusResponse>.Ok(response));
    }
}
