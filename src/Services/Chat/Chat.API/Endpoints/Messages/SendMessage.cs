using NovaCore.BuildingBlock.Application.Abstractions.Common;
using NovaCore.BuildingBlock.SharedKernel.Extensions;

using NovaCore.Chat.Application.Features.Messages.Commands.SendMessage;
using NovaCore.Chat.Application.Features.Messages.DTOs;

namespace NovaCore.Chat.API.Endpoints.Messages;

public sealed record SendMessageAttachmentRequest(
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    int? Width = null,
    int? Height = null,
    int? DurationSeconds = null);

public sealed record SendMessageRequest(
    string ClientMessageId,
    MessageType Type,
    string Content,
    MessageFormat Format = MessageFormat.PlainText,
    List<SendMessageAttachmentRequest>? Attachments = null);

/// <summary>
/// REST send path - the created message is also broadcast through ChatHub (see
/// SignalRChatRealtimeNotifier), including back to the sender, so the client can reconcile its
/// local state off the same canonical event every other participant receives.
/// </summary>
public sealed class SendMessageEndpoint : ICarterModule
{
    private readonly string[] API_DESC = [
        "## Send Message",
        "",
        "Sends a message (optionally with attachments already uploaded elsewhere) to a",
        "conversation. The response includes the assigned per-conversation Sequence, which the",
        "client uses to detect gaps and trigger recovery through ChatHub.RecoverMessages.",
        "",
        "### Error Responses",
        "- **400**: Invalid request, or the conversation is closed",
        "- **403**: Caller is not a participant/contact of this conversation",
        "- **404**: Conversation not found",
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations/{conversationId}/messages", Handle)
            .WithTags("Messages")
            .RequireAuthorization()
            .WithName("SendMessage")
            .WithDisplayName("Send Message API")
            .WithDescription(API_DESC.JoinToString("\n"))
            .Produces<ApiResponse<SendMessageResponse>>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid conversationId,
        [FromBody] SendMessageRequest request,
        [FromServices] ISender sender,
        CancellationToken ct = default)
    {
        var attachments = request.Attachments?
            .Select(a => new MessageAttachmentRequest(a.FileName, a.ContentType, a.SizeBytes, a.StorageKey, a.Width, a.Height, a.DurationSeconds))
            .ToList();

        var command = new SendMessageCommand(
            conversationId,
            request.ClientMessageId.Trim(),
            request.Type,
            request.Content,
            request.Format,
            attachments);

        var response = await sender.Send(command, ct);

        return Results.Created($"/conversations/{conversationId}/messages/{response.MessageId}",
            ApiResponse<SendMessageResponse>.Ok(response));
    }
}
