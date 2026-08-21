using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.Conversations.Queries.GetConversation;

namespace NovaCore.Chat.API.Endpoints.Conversations;

public sealed class GetConversationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Get Conversation Details",
        "",
        "Retrieves conversation information by conversation ID.",
        "",
        "### Route Parameters",
        "- **conversationId**: Unique identifier of the conversation (required, must be valid GUID)",
        "",
        "### Error Responses",
        "- **404**: Conversation not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/conversations/{conversationId}", Handle)
            .WithTags("Conversations")
            .RequireAuthorization()
            .WithName("GetConversation")
            .WithDisplayName("Get Conversation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<GetConversationResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid conversationId,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var query = new GetConversationQuery(conversationId);
        var response = await sender.Send(query, ct);

        return Results.Ok(ApiResponse<GetConversationResponse>.Ok(response));
    }
}
