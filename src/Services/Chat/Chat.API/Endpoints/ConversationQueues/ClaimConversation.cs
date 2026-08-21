using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.ConversationQueues.Commands.ClaimConversation;

namespace NovaCore.Chat.API.Endpoints.ConversationQueues;

public sealed class ClaimConversationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Claim Conversation",
        "",
        "CS/admin picks a waiting conversation from the queue - marks the queue item Assigned,",
        "creates a ConversationAssignment for the caller, and opens the conversation if it was",
        "still Queued.",
        "",
        "### Error Responses",
        "- **403**: Caller is a guest",
        "- **404**: Conversation not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations/{conversationId}/claim", Handle)
            .WithTags("ConversationQueues")
            .RequireAuthorization()
            .WithName("ClaimConversation")
            .WithDisplayName("Claim Conversation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid conversationId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        await sender.Send(new ClaimConversationCommand(conversationId), ct);

        return Results.NoContent();
    }
}
