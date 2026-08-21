using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.ConversationReasonSuggestions.Commands.CreateConversationReasonSuggestion;

namespace NovaCore.Chat.API.Endpoints.ConversationReasonSuggestions;

public sealed record CreateConversationReasonSuggestionRequest(string Code, int SortOrder = 0);

/// <summary>Admin-managed catalog entry - see ConversationReasonSuggestion.cs. No dedicated admin-role gate exists in Chat yet, same limitation as every other admin-facing endpoint in this service.</summary>
public sealed class CreateConversationReasonSuggestionEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Conversation Reason Suggestion",
        "",
        "Admin-managed predefined reason a guest/user can pick from when starting a conversation.",
        "Localize it afterwards via Translate Conversation Reason Suggestion.",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
        "- **409**: Code already in use",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/conversation-reason-suggestions", Handle)
            .WithTags("ConversationReasonSuggestions")
            .RequireAuthorization()
            .WithName("CreateConversationReasonSuggestion")
            .WithDisplayName("Create Conversation Reason Suggestion API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateConversationReasonSuggestionResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateConversationReasonSuggestionRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateConversationReasonSuggestionCommand(request.Code.Trim(), request.SortOrder);

        var response = await sender.Send(command, ct);

        return Results.Created($"/conversation-reason-suggestions/{response.Id}",
            ApiResponse<CreateConversationReasonSuggestionResponse>.Ok(response));
    }
}
