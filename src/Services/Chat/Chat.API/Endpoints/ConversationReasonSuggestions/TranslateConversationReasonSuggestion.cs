using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Commands.TranslateConversationReasonSuggestion;

namespace NovaCore.Chat.API.Endpoints.ConversationReasonSuggestions;

public sealed record TranslateConversationReasonSuggestionRequest(string LanguageCode, string Text);

public sealed class TranslateConversationReasonSuggestionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Translate Conversation Reason Suggestion",
        "",
        "Upserts the phrase shown for one language on an existing reason suggestion.",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **404**: Suggestion not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/conversation-reason-suggestions/{suggestionId}/translations", Handle)
            .WithTags("ConversationReasonSuggestions")
            .RequireAuthorization()
            .WithName("TranslateConversationReasonSuggestion")
            .WithDisplayName("Translate Conversation Reason Suggestion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid suggestionId,
        [FromBody] TranslateConversationReasonSuggestionRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new TranslateConversationReasonSuggestionCommand(
            suggestionId,
            request.LanguageCode.Trim(),
            request.Text.Trim());

        await sender.Send(command, ct);

        return Results.NoContent();
    }
}
