using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.Conversations.Commands.StartGuestConversation;

namespace NovaCore.Chat.API.Endpoints.Guests;

public sealed record StartGuestConversationRequest(
    string DisplayName,
    string? Email = null,
    string? Phone = null,
    string? Reason = null);

/// <summary>
/// Spec section 39's guest entry point - a guest submits basic contact info before starting a
/// chat and, in exchange, gets back a short-lived identity (as both an httpOnly cookie, reusing
/// the exact same "AccessToken" cookie every authenticated login already sets via
/// ICurrentUserService, and in the response body for callers that need to pass it as a Bearer
/// token instead, e.g. the ChatHub connection or a cross-origin SPA).
/// </summary>
public sealed class StartGuestConversationEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Start Guest Conversation",
        "",
        "Guest entry point - no authentication required. Submits basic contact information,",
        "creates a Contact + a Queued Session conversation, and returns a short-lived access",
        "token (also set as the AccessToken cookie) the guest can use for subsequent REST calls",
        "and the ChatHub connection.",
        "",
        "### Error Responses",
        "- **400**: Invalid request or validation failed",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/guests/conversations", Handle)
            .WithTags("Guests")
            .AllowAnonymous()
            .WithName("StartGuestConversation")
            .WithDisplayName("Start Guest Conversation API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<StartGuestConversationResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromBody] StartGuestConversationRequest request,
        [FromServices] ISender sender,
        [FromServices] ICurrentUserService currentUser,
        CancellationToken ct = default)
    {
        var command = new StartGuestConversationCommand(
            request.DisplayName.Trim(),
            request.Email?.Trim(),
            request.Phone?.Trim(),
            request.Reason?.Trim());

        var response = await sender.Send(command, ct);

        currentUser.SetAccessToken(response.AccessToken);

        return Results.Created($"/conversations/{response.ConversationId}",
            ApiResponse<StartGuestConversationResponse>.Ok(response));
    }
}
