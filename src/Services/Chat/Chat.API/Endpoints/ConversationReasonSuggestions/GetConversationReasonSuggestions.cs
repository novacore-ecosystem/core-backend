using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Queries.GetConversationReasonSuggestions;

namespace NovaCore.Chat.API.Endpoints.ConversationReasonSuggestions;

/// <summary>Public-facing - guests need this before a conversation (and its access token) exist.</summary>
public sealed class GetConversationReasonSuggestionsEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Conversation Reason Suggestions",
        "",
        "Active reason suggestions translated into the given language, for the client's",
        "conversation-start reason picker. Suggestions are only shortcuts - the conversation",
        "always stores whatever plain text the user ends up submitting.",
        "",
        "### Query Parameters",
        "- **language** (required): BCP-47 language code, e.g. `en`",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/conversation-reason-suggestions", Handle)
            .WithTags("ConversationReasonSuggestions")
            .AllowAnonymous()
            .WithName("GetConversationReasonSuggestions")
            .WithDisplayName("Get Conversation Reason Suggestions API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<IReadOnlyList<ConversationReasonSuggestionDto>>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromQuery(Name = "language")] string language,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var response = await sender.Send(new GetConversationReasonSuggestionsQuery(language), ct);

        return Results.Ok(ApiResponse<IReadOnlyList<ConversationReasonSuggestionDto>>.Ok(response));
    }
}
