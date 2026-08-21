using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.Conversations.Commands.CloseConversation;

namespace NovaCore.Chat.API.Endpoints.Conversations;

public sealed class CloseConversationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Close Conversation",
        "",
        "Explicitly closes a conversation. Callable by the guest/user who owns it, or any",
        "authenticated internal user (CS). Broadcasts ConversationClosed to the conversation's",
        "SignalR group.",
        "",
        "### Error Responses",
        "- **400**: Conversation is already closed",
        "- **403**: Caller is not a participant/contact of this conversation",
        "- **404**: Conversation not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations/{conversationId}/close", Handle)
            .WithTags("Conversations")
            .RequireAuthorization()
            .WithName("CloseConversation")
            .WithDisplayName("Close Conversation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid conversationId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        await sender.Send(new CloseConversationCommand(conversationId), ct);

        return Results.NoContent();
    }
}
