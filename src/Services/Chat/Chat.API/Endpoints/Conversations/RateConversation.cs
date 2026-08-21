using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.Conversations.Commands.RateConversation;

namespace NovaCore.Chat.API.Endpoints.Conversations;

public sealed record RateConversationRequest(byte Stars, string? Review = null);

/// <summary>
/// Star rating (0-5) plus an optional review. 0 stars with no review means "not rated" (no row
/// created below this point isn't observable from the client - the distinction only matters
/// server-side, see ConversationRating.cs); 0 stars submitted through this endpoint is a genuine
/// zero-star rating.
/// </summary>
public sealed class RateConversationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Rate Conversation",
        "",
        "Submits quality feedback (star rating + optional review) for a closed conversation.",
        "At most one rating per conversation.",
        "",
        "### Error Responses",
        "- **400**: Invalid request, or the conversation is not closed yet",
        "- **403**: Caller is not a participant/contact of this conversation",
        "- **404**: Conversation not found",
        "- **409**: Conversation was already rated",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations/{conversationId}/rating", Handle)
            .WithTags("Conversations")
            .RequireAuthorization()
            .WithName("RateConversation")
            .WithDisplayName("Rate Conversation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<RateConversationResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid conversationId,
        [FromBody] RateConversationRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new RateConversationCommand(conversationId, request.Stars, request.Review?.Trim());

        var response = await sender.Send(command, ct);

        return Results.Created($"/conversations/{conversationId}/rating",
            ApiResponse<RateConversationResponse>.Ok(response));
    }
}
