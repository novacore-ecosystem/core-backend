using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.Conversations.Commands.CreateConversation;

namespace NovaCore.Chat.API.Endpoints.Conversations;

public sealed record CreateConversationRequest(
    ConversationType Type,
    ConversationLifecycle Lifecycle,
    string? Title = null,
    string? Description = null,
    string? Avatar = null,
    ConversationPriority Priority = ConversationPriority.Normal);

public sealed class CreateConversationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Create Conversation",
        "",
        "Creates a new conversation (1:1 or Group, Session or Persistent).",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations", Handle)
            .WithTags("Conversations")
            .RequireAuthorization()
            .WithName("CreateConversation")
            .WithDisplayName("Create Conversation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<CreateConversationResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateConversationRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var command = new CreateConversationCommand(
            request.Type,
            request.Lifecycle,
            request.Title?.Trim(),
            request.Description?.Trim(),
            request.Avatar?.Trim(),
            request.Priority);

        var response = await sender.Send(command, ct);

        return Results.Created($"/conversations/{response.ConversationId}",
            ApiResponse<CreateConversationResponse>.Ok(response));
    }
}
